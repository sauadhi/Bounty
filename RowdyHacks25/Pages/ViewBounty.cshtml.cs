using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Models;
using RowdyHacks25.Data;
using RowdyHacks25.Services;

namespace RowdyHacks25.Pages
{
    public class ViewBountyModel : PageModel
    {
        private readonly IBountyRepository _repo;
        private readonly IGeminiSummarizer _summarizer;

        public Bounty? Bounty { get; private set; }
        public bool IsOwner { get; private set; }
        public bool AiAvailable { get; private set; }

        public ViewBountyModel(IBountyRepository repo, IGeminiSummarizer summarizer)
        {
            _repo = repo;
            _summarizer = summarizer;
        }

        public IActionResult OnGet(int? id)
        {
            if (!id.HasValue)
                return RedirectToPage("/Bounties");

            Bounty = _repo.GetById(id.Value);
            if (Bounty == null)
                return RedirectToPage("/Bounties");

            string? currentUserId = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                IsOwner = Bounty.ManagerId == OwnerKey.DeriveManagerId(currentUserId);
            }

            AiAvailable = _summarizer.IsConfigured;
            return Page();
        }

        public IActionResult OnPostDelete()
        {
            var id = RouteData.Values["id"];
            if (id == null) return RedirectToPage("/Bounties");

            var bountyId = int.Parse(id.ToString()!);
            var bounty = _repo.GetById(bountyId);
            if (bounty == null) return RedirectToPage("/Bounties");

            string? currentUserId = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(currentUserId) && bounty.ManagerId == OwnerKey.DeriveManagerId(currentUserId))
            {
                _repo.Remove(bountyId);
            }

            return RedirectToPage("/Bounties");
        }

        // AJAX: generate summary once and persist
        public async Task<IActionResult> OnPostSummarizeAsync(int id, CancellationToken ct)
        {
            var bounty = _repo.GetById(id);
            if (bounty == null)
                return new JsonResult(new { error = "Not found" }) { StatusCode = 404 };

            if (!_summarizer.IsConfigured)
                return new JsonResult(new { error = "AI summarization is not configured." }) { StatusCode = 503 };

            // Only generate once
            if (!string.IsNullOrWhiteSpace(bounty.Summary))
                return new JsonResult(new { alreadyGenerated = true, summary = bounty.Summary });

            try
            {
                var summary = await _summarizer.SummarizeAsync(bounty, ct);
                bounty.Summary = summary?.Trim();
                _repo.Update(bounty);
                return new JsonResult(new { summary = bounty.Summary });
            }
            catch (OperationCanceledException)
            {
                return new JsonResult(new { error = "Summarization canceled." }) { StatusCode = 499 };
            }
            catch (InvalidOperationException ex)
            {
                return new JsonResult(new { error = ex.Message }) { StatusCode = 503 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = $"Unexpected error: {ex.Message}" }) { StatusCode = 500 };
            }
        }
    }
}

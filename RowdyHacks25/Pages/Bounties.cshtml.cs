using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Data;
using RowdyHacks25.Models;

namespace RowdyHacks25.Pages
{
    public class BountiesModel : PageModel
    {
        private readonly IBountyRepository _repo;

        public BountiesModel(IBountyRepository repo) => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        // List + Search
        public List<Bounty> Bounties { get; private set; } = new();

        // GET query (?q=...) and pagination (?pageNumber=...)
        [BindProperty(SupportsGet = true)]
        public string? q { get; set; }

        [BindProperty(SupportsGet = true)]
        public int pageNumber { get; set; } = 1;

        private const int PageSize = 10;

        // Count for the banner
        public int NumberOfBounties { get; private set; }

        // Pagination info
        public int TotalPages { get; private set; }

        // Create form binding
        [BindProperty]
        public Bounty NewBounty { get; set; } = new();

        public void OnGet()
        {
            Load();
        }

        public IActionResult OnPostCreate()
        {
            if (!ModelState.IsValid)
            {
                Load();
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewBounty.Status)) NewBounty.Status = "Open";
            if (string.IsNullOrWhiteSpace(NewBounty.DangerLevel)) NewBounty.DangerLevel = "Low";

            _repo.Add(NewBounty);
            return RedirectToPage(new { q, pageNumber });
        }

        public IActionResult OnPostDelete(int id)
        {
            _repo.Remove(id);
            return RedirectToPage(new { q, pageNumber });
        }

        public bool IsOwner(string? managerId)
        {
            if (string.IsNullOrEmpty(managerId)) return false;
            string? currentUserId = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(currentUserId))
                return managerId == OwnerKey.DeriveManagerId(currentUserId);
            return false;
        }

        private void Load()
        {
            var all = _repo.GetAll() ?? new List<Bounty>();

            // --- Filter ---
            if (!string.IsNullOrWhiteSpace(q))
            {
                all = all.Where(b =>
                    (b.TargetName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (b.Planet?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (b.PostedBy?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (b.DangerLevel?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (b.Status?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            // --- Sort ---
            all = all.OrderByDescending(b => b.Reward)
                     .ThenBy(b => b.TargetName)
                     .ToList();

            // --- Pagination ---
            NumberOfBounties = all.Count;
            TotalPages = (int)Math.Ceiling(NumberOfBounties / (double)PageSize);
            pageNumber = Math.Clamp(pageNumber, 1, Math.Max(TotalPages, 1));

            Bounties = all.Skip((pageNumber - 1) * PageSize)
                          .Take(PageSize)
                          .ToList();
        }
    }
}

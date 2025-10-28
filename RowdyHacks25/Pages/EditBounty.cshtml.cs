using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Models;
using RowdyHacks25.Data;

namespace RowdyHacks25.Pages
{
    public class EditBountyModel : PageModel
    {
        private readonly IBountyRepository _repo;

        [BindProperty]
        public Bounty EditingBounty { get; set; } = new();

        [BindProperty]
        public string? OwnerKeyInput { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public EditBountyModel(IBountyRepository repo) => _repo = repo;

        public IActionResult OnGet(int? id)
        {
            if (!id.HasValue)
                return RedirectToPage("/Bounties");

            var bounty = _repo.GetById(id.Value);
            if (bounty == null)
                return RedirectToPage("/Bounties");

            EditingBounty = bounty;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            var existingBounty = _repo.GetById(EditingBounty.Id);
            if (existingBounty == null)
            {
                ErrorMessage = "Bounty not found.";
                return RedirectToPage("/Bounties");
            }

            // Only verify ownership if the bounty has an owner
            if (!string.IsNullOrEmpty(existingBounty.ManagerId))
            {
                if (string.IsNullOrEmpty(OwnerKeyInput))
                {
                    ErrorMessage = "Owner key is required to edit this bounty.";
                    EditingBounty = existingBounty;
                    return Page();
                }

                string hashedKey = OwnerKey.DeriveManagerId(OwnerKeyInput);
                if (hashedKey != existingBounty.ManagerId)
                {
                    ErrorMessage = "Invalid owner key.";
                    EditingBounty = existingBounty;
                    return Page();
                }
            }

            // Update only allowed fields
            existingBounty.TargetName = EditingBounty.TargetName;
            existingBounty.Planet = EditingBounty.Planet;
            existingBounty.Reward = EditingBounty.Reward;
            existingBounty.DangerLevel = EditingBounty.DangerLevel;
            existingBounty.ImageUrl = EditingBounty.ImageUrl;

            _repo.Update(existingBounty);

            return RedirectToPage("/ViewBounty", new { id = existingBounty.Id });
        }

        // ===== DELETE HANDLER =====
        public IActionResult OnPostDelete(int id)
        {
            if (id <= 0) return RedirectToPage("/Bounties");

            var bounty = _repo.GetById(id);
            if (bounty == null) return RedirectToPage("/Bounties");

            // If there IS an owner, require the correct owner key (hashed compare)
            if (!string.IsNullOrEmpty(bounty.ManagerId))
            {
                if (string.IsNullOrEmpty(OwnerKeyInput) ||
                    OwnerKey.DeriveManagerId(OwnerKeyInput) != bounty.ManagerId)
                {
                    ErrorMessage = "Invalid or missing owner key — cannot delete.";
                    EditingBounty = bounty; // keep the page populated so the user can retry
                    return Page();
                }
            }
            // else: if no owner, this allows anyone to delete.
            // To block deletion when unowned, uncomment the next 4 lines:
            // if (string.IsNullOrEmpty(bounty.ManagerId)) {
            //     ErrorMessage = "This bounty has no owner; deletion is disabled.";
            //     EditingBounty = bounty; return Page();
            // }

            _repo.Remove(id);
            TempData["Toast"] = "Bounty deleted.";
            return RedirectToPage("/Bounties");
        }
    }
}

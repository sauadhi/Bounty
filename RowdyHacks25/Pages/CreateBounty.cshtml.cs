using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Models;
using RowdyHacks25.Data;

namespace RowdyHacks25.Pages
{
    public class CreateBountyModel : PageModel
    {
        private readonly IBountyRepository _repo;

        [BindProperty]
        public Bounty NewBounty { get; set; } = new();

        public CreateBountyModel(IBountyRepository repo) => _repo = repo;

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            // Use the provided image URL or fall back to a placeholder
            if (string.IsNullOrWhiteSpace(NewBounty.ImageUrl))
                NewBounty.ImageUrl = "/images/question-mark.png";
                
            // Set and hash the manager/owner ID
            string rawManagerId = NewBounty.ManagerId ?? User?.Identity?.Name ?? "anonymous";
            NewBounty.ManagerId = OwnerKey.DeriveManagerId(rawManagerId);
            
            // Set the displayed posted by name (non-hashed)
            NewBounty.PostedBy = !string.IsNullOrWhiteSpace(NewBounty.ManagerId) ? "Verified Owner" : "Anonymous";

            _repo.Add(NewBounty);
            return RedirectToPage("/Bounties");
        }
    }
}

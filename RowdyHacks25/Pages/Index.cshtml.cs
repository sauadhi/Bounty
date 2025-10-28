using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Data;
using RowdyHacks25.Models;

namespace RowdyHacks25.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IBountyRepository _repo;

        public IndexModel(IBountyRepository repo) => _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        // List + Search
        public List<Bounty> Bounties { get; private set; } = new();

        // GET query (?q=...)
        [BindProperty(SupportsGet = true)]
        public string? q { get; set; }

        // Count for the banner
        public int NumberOfBounties { get; private set; }

        // Create form binding
        [BindProperty]
        public Bounty NewBounty { get; set; } = new();

        public void OnGet()
        {
            Load();
        }

        // Handle the "Create" form submit
        public IActionResult OnPostCreate()
        {
            if (!ModelState.IsValid)
            {
                Load();
                return Page();
            }

            // sensible defaults
            if (string.IsNullOrWhiteSpace(NewBounty.Status)) NewBounty.Status = "Open";
            if (string.IsNullOrWhiteSpace(NewBounty.DangerLevel)) NewBounty.DangerLevel = "Low";

            _repo.Add(NewBounty);

            // Redirect to GET so refresh doesn't re-post; preserve current search if any
            return RedirectToPage(new { q });
        }

        // Optional: delete handler (tiny admin)
        public IActionResult OnPostDelete(int id)
        {
            _repo.Remove(id);
            return RedirectToPage(new { q });
        }

        private void Load()
        {
            var all = _repo.GetAll() ?? new List<Bounty>();

            // Get top 3 highest paying bounties that are still open
            Bounties = all
                .Where(b => b.Status == "Open")
                .OrderByDescending(b => b.Reward)
                .Take(3)
                .ToList();

            NumberOfBounties = Bounties.Count;
        }
    }
}

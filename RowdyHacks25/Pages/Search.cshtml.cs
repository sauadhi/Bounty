using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Data;
using RowdyHacks25.Models;

namespace RowdyHacks25.Pages
{
    public class SearchModel : PageModel
    {
        private IBountyRepository _repo;

        public SearchModel(IBountyRepository repo)
        {
            if(repo == null)
            {
                throw new ArgumentNullException(nameof(repo));
            }
            _repo = repo;
        }

        [BindProperty(SupportsGet = true)]
        public String? q { get; set; }

        public List<Bounty>? bounties { get; private set; } = new();

        public int numberOfBounties { get; private set; }
        public void OnGet()
        {
            Load();
        }

        private void Load()
        {
            if (!String.IsNullOrWhiteSpace(q))
            {
                var all = _repo.GetAll() ?? new List<Bounty>();
                bounties = all
                    .Where(b => b.TargetName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                b.Planet.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                b.DangerLevel.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                b.Status.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                b.PostedBy.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                                (b.ClaimedBy != null && b.ClaimedBy.Contains(q, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(b => b.Reward)
                    .ThenBy(b => b.TargetName)
                    .ToList();
                numberOfBounties = bounties.Count;
            } else
            {
                bounties = null;
                numberOfBounties = 0;
            }
        }
    }
}

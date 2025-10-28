using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RowdyHacks25.Data;

namespace RowdyHacks25.Pages
{
    public class UserPageModel : PageModel
    {
        private readonly IUserRepository _repo;

        public UserPageModel(IUserRepository repo) => _repo = repo;

        public List<RowdyHacks25.Models.User> Users { get; private set; } = new();

        public void OnGet()
        {
            Users = _repo.GetAll();
        }

        public IActionResult OnPostDelete(int id)
        {
            _repo.Remove(id);
            return RedirectToPage();
        }
    }
}

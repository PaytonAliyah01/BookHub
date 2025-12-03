using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    public class DashboardModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        public DashboardModel(AdminBLL adminBLL)
        {
            _adminBLL = adminBLL;
        }
        public Dictionary<string, object> SystemStats { get; set; } = new();
        public List<User> RecentUsers { get; set; } = new();
        public List<Book> RecentBooks { get; set; } = new();
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                SystemStats = _adminBLL.GetSystemStats();
                RecentUsers = _adminBLL.GetAllUsers().Take(5).ToList();
                RecentBooks = _adminBLL.GetAllBooks().Take(5).ToList();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
            }
            return Page();
        }
    }
}
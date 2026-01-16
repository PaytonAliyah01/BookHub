using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    [IgnoreAntiforgeryToken]
    public class UsersModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        public UsersModel(AdminBLL adminBLL)
        {
            _adminBLL = adminBLL;
        }
        public List<User> Users { get; set; } = new();
        public Dictionary<string, object> SystemStats { get; set; } = new();
        public Dictionary<int, int> UserBookCounts { get; set; } = new();
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                Users = _adminBLL.GetAllUsers();
                SystemStats = _adminBLL.GetSystemStats();
                
                // Get book count for each user
                foreach (var user in Users)
                {
                    var books = _adminBLL.GetUserBooks(user.UserId);
                    UserBookCounts[user.UserId] = books?.Count ?? 0;
                }
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error loading users: {ex.Message}";
            }
            return Page();
        }
        public IActionResult OnPostDeleteUser(int userId)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Users.cshtml.cs] Attempting to delete user {userId}");
                bool success = _adminBLL.DeleteUser(userId);
                System.Diagnostics.Debug.WriteLine($"[Users.cshtml.cs] Delete result: {success}");
                
                if (success)
                {
                    TempData["Message"] = "User deleted successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to delete user - delete returned false. Check console output for details.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Users.cshtml.cs] Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Users.cshtml.cs] InnerException: {ex.InnerException.Message}");
                }
                
                var innerMessage = ex.InnerException != null ? " Inner: " + ex.InnerException.Message : "";
                TempData["Message"] = "Error deleting user: " + ex.Message + innerMessage;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage("/Admin/Users");
        }
        public IActionResult OnPostRestrictUser(int userId, bool isRestricted)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                bool success = _adminBLL.RestrictUser(userId, isRestricted);
                if (success)
                {
                    string action = isRestricted ? "restricted" : "unrestricted";
                    TempData["Message"] = $"User {action} successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to update user restriction.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error updating user restriction: " + ex.Message;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage();
        }
    }
}
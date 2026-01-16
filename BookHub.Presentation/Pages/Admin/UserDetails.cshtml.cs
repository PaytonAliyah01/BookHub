using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    [IgnoreAntiforgeryToken]
    public class UserDetailsModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        public UserDetailsModel(AdminBLL adminBLL)
        {
            _adminBLL = adminBLL;
        }
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }
        public new User? User { get; set; }
        public List<Book>? UserBooks { get; set; }
        public string ErrorMessage { get; set; } = "";
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                var users = _adminBLL.GetAllUsers();
                User = users.FirstOrDefault(u => u.UserId == UserId);
                if (User == null)
                {
                    ErrorMessage = "User not found.";
                    return Page();
                }
                
                UserBooks = _adminBLL.GetUserBooks(UserId);
                
                if (UserBooks == null)
                {
                    UserBooks = new List<Book>();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading user details: {ex.Message}";
                UserBooks = new List<Book>();
            }
            return Page();
        }
        public IActionResult OnPostRestrictUser(int userId)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                bool success = _adminBLL.RestrictUser(userId, true);
                if (success)
                {
                    TempData["Message"] = "User restricted successfully!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to restrict user.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error restricting user: {ex.Message}";
                TempData["MessageType"] = "error";
            }
            return RedirectToPage(new { userId = UserId });
        }
        public IActionResult OnPostDeleteUser(int userId)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                System.Diagnostics.Debug.WriteLine($"Attempting to delete user {userId}");
                bool success = _adminBLL.DeleteUser(userId);
                System.Diagnostics.Debug.WriteLine($"Delete result: {success}");
                if (success)
                {
                    TempData["Message"] = "User deleted successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToPage("/Admin/Users");
                }
                else
                {
                    TempData["Message"] = "Failed to delete user - delete returned false.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during delete: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                var innerMessage = ex.InnerException != null ? " Details: " + ex.InnerException.Message : "";
                TempData["Message"] = "Error deleting user: " + ex.Message + innerMessage;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage(new { userId = UserId });
        }
    }
}

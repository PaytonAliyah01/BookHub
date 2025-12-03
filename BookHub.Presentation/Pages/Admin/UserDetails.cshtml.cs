using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
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
                bool success = _adminBLL.DeleteUser(userId);
                if (success)
                {
                    TempData["Message"] = "User deleted successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToPage("/Admin/Users");
                }
                else
                {
                    TempData["Message"] = "Failed to delete user.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error deleting user: {ex.Message}";
                TempData["MessageType"] = "error";
            }
            return RedirectToPage(new { userId = UserId });
        }
    }
}

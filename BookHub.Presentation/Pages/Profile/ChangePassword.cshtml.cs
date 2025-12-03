using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookHub.BLL;
namespace BookHub.Presentation.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IUserBLL _userBLL;
        public ChangePasswordModel(IUserBLL userBLL)
        {
            _userBLL = userBLL;
        }
        [BindProperty]
        public string CurrentPassword { get; set; } = "";
        [BindProperty]
        public string NewPassword { get; set; } = "";
        [BindProperty]
        public string ConfirmPassword { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    ErrorMessage = "Unable to identify current user. Please log in again.";
                    return Page();
                }
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                {
                    ErrorMessage = "Current password is required.";
                    return Page();
                }
                if (string.IsNullOrWhiteSpace(NewPassword))
                {
                    ErrorMessage = "New password is required.";
                    return Page();
                }
                if (NewPassword != ConfirmPassword)
                {
                    ErrorMessage = "New password and confirmation do not match.";
                    return Page();
                }
                if (!_userBLL.IsPasswordStrong(NewPassword))
                {
                    ErrorMessage = "Password must be at least 8 characters and contain uppercase, lowercase, number, and special character.";
                    return Page();
                }
                if (_userBLL.ChangePassword(email, CurrentPassword, NewPassword))
                {
                    SuccessMessage = "Password changed successfully!";
                    CurrentPassword = "";
                    NewPassword = "";
                    ConfirmPassword = "";
                }
                else
                {
                    ErrorMessage = "Unable to change password. Please check your current password and try again.";
                }
                return Page();
            }
            catch (ApplicationException ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred. Please try again later.";
                return Page();
            }
        }
    }
}

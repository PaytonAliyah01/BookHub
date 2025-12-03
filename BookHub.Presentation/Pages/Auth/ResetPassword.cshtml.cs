using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
namespace BookHub.Presentation.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IUserBLL _userBLL;
        public ResetPasswordModel(IUserBLL userBLL)
        {
            _userBLL = userBLL;
        }
        [BindProperty]
        public string Email { get; set; } = "";
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? TemporaryPassword { get; set; }
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Email address is required.";
                    return Page();
                }
                if (!_userBLL.UserExists(Email))
                {
                    SuccessMessage = "If this email is registered, a temporary password has been generated. Please check below.";
                    return Page();
                }
                TemporaryPassword = _userBLL.GenerateTemporaryPassword(10);
                if (_userBLL.ResetPassword(Email, TemporaryPassword))
                {
                    SuccessMessage = $"Password has been reset successfully! Your temporary password is: <strong>{TemporaryPassword}</strong><br/>Please change it after logging in.";
                    Email = ""; 
                }
                else
                {
                    ErrorMessage = "Unable to reset password. Please try again later.";
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;

namespace BookHub.Presentation.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserBLL _userBLL;

        public ResetPasswordModel(IConfiguration config)
        {
            _userBLL = new UserBLL(config.GetConnectionString("DefaultConnection")!);
        }

        [BindProperty]
        public string Email { get; set; } = "";

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? TemporaryPassword { get; set; }

        public void OnGet()
        {
            // Page loads empty for password reset form
        }

        public IActionResult OnPost()
        {
            try
            {
                // Presentation layer validation
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Email address is required.";
                    return Page();
                }

                // Check if user exists (business logic)
                if (!_userBLL.UserExists(Email))
                {
                    // For security, don't reveal whether email exists or not
                    SuccessMessage = "If this email is registered, a temporary password has been generated. Please check below.";
                    return Page();
                }

                // Generate temporary password
                TemporaryPassword = _userBLL.GenerateTemporaryPassword(10);

                // Reset password with temporary password
                if (_userBLL.ResetPassword(Email, TemporaryPassword))
                {
                    SuccessMessage = $"Password has been reset successfully! Your temporary password is: <strong>{TemporaryPassword}</strong><br/>Please change it after logging in.";
                    Email = ""; // Clear email field
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
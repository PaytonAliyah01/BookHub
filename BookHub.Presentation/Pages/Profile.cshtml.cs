using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookHub.BLL;

namespace BookHub.Presentation.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserBLL _userBLL;

        public ProfileModel(IConfiguration config)
        {
            _userBLL = new UserBLL(config.GetConnectionString("DefaultConnection")!);
        }

        public BookHub.DAL.User? UserProfile { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToPage("/Login");
                }

                // Load user profile
                UserProfile = _userBLL.GetUserProfile(email);
                if (UserProfile == null)
                {
                    ErrorMessage = "Unable to load user profile.";
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
                ErrorMessage = "An unexpected error occurred while loading your profile.";
                return Page();
            }
        }
    }
}
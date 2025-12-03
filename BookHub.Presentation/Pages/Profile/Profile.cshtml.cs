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
        private readonly IUserBLL _userBLL;
        public ProfileModel(IUserBLL userBLL)
        {
            _userBLL = userBLL;
        }
        public UserDto? UserProfile { get; set; }
        public string? ErrorMessage { get; set; }
        public IActionResult OnGet()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToPage("/Auth/Login");
                }
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

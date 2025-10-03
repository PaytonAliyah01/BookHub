using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.Services;
using BookHub.Models;
using Microsoft.AspNetCore.Http;

namespace BookHub.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;

        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public string? Email { get; set; }
        [BindProperty]
        public string? Password { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }

            var user = await _userService.AuthenticateAsync(Email, Password);
            if (user != null)
            {
                // Store user ID in session
                HttpContext.Session.SetInt32("UserId", user.UserId);
                return RedirectToPage("/Dashboard/Index");
            }

            ErrorMessage = "Invalid email or password.";
            return Page();
        }
    }
}

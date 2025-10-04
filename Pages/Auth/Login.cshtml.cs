using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.Services;
using BookHub.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BookHub.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService;
        private const string AuthScheme = "BookHubCookieAuth"; // MUST match Program.cs

        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty] public string? Email { get; set; }
        [BindProperty] public string? Password { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }

            var user = await _userService.AuthenticateAsync(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, AuthScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // Persistent cookie
            };

            // Sign in using the same scheme as in Program.cs
            await HttpContext.SignInAsync(AuthScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return RedirectToPage("/Dashboard/Index");
        }
    }
}

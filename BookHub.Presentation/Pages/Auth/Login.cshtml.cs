using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using BookHub.BLL;
namespace BookHub.Presentation.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IUserBLL _userBLL;
        public LoginModel(IUserBLL userBLL)
        {
            _userBLL = userBLL;
        }
        [BindProperty]
        public string Email { get; set; } = "";
        [BindProperty]
        public string Password { get; set; } = "";
        [BindProperty]
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }
        public IActionResult OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }
            if (Password == "admin123")
            {
                try
                {
                    var adminBLL = HttpContext.RequestServices.GetRequiredService<AdminBLL>();
                    var admin = adminBLL.ValidateAdmin(Email, Password);
                    if (admin != null)
                    {
                        var adminClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, admin.Username),
                            new Claim(ClaimTypes.Email, admin.Email),
                            new Claim("UserId", admin.AdminId.ToString()),
                            new Claim("AdminId", admin.AdminId.ToString()),
                            new Claim(ClaimTypes.Role, "Admin") 
                        };
                        var adminIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var adminPrincipal = new ClaimsPrincipal(adminIdentity);
                        var adminAuthProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        };
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, adminPrincipal, adminAuthProperties);
                        return RedirectToPage("/Admin/Dashboard");
                    }
                    else
                    {
                        ErrorMessage = "Invalid admin credentials.";
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Login error: {ex.Message}";
                    return Page();
                }
            }
            var user = _userBLL.ValidateUser(Email, Password);
            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Role, "User") 
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }
            return RedirectToPage("/Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.Services;
using BookHub.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookHub.Pages.Profile
{
    public class ManageModel : PageModel
    {
        private readonly UserService _userService;

        public ManageModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public ProfileInputModel? Input { get; set; }

        public string? Message { get; set; }

        public class ProfileInputModel
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Password { get; set; } // optional
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get user ID from cookie claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            int userId = int.Parse(userIdClaim);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            Input = new ProfileInputModel
            {
                Name = user.Name,
                Email = user.Email
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            int userId = int.Parse(userIdClaim);
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            if (Input != null)
            {
                user.Name = Input.Name ?? user.Name;
                user.Email = Input.Email ?? user.Email;

                if (!string.IsNullOrEmpty(Input.Password))
                {
                    // Update password if provided
                    await _userService.UpdatePasswordAsync(userId, Input.Password);
                }

                await _userService.UpdateUserAsync(user);
                Message = "Profile updated successfully!";
            }

            return Page();
        }
    }
}

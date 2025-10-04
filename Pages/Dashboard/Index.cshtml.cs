using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookHub.Pages.Dashboard
{
    [Authorize] // Ensures only logged-in users can access
    public class IndexModel : PageModel
    {
        public string UserName { get; set; } = "Guest";

        public IActionResult OnGet()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Get the user's name from claims
                UserName = User.FindFirstValue(ClaimTypes.Name) ?? "User";
            }

            return Page();
        }
    }
}


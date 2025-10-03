using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace BookHub.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        public string UserName { get; set; } = "Guest";

        public IActionResult OnGet()
        {
            // Example: Get user name from session (or claims if using Identity later)
            var name = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(name))
            {
                UserName = name;
            }

            return Page();
        }
    }
}

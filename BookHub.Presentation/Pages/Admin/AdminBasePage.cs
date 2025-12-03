using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace BookHub.Presentation.Pages.Admin
{
    public abstract class AdminBasePage : PageModel
    {
        protected bool IsAdminAuthenticated()
        {
            return User.Identity?.IsAuthenticated == true && User.HasClaim(c => c.Type == "AdminId");
        }
        public string GetAdminUsername()
        {
            return User.Identity?.Name ?? "Admin";
        }
        protected int? GetAdminId()
        {
            var adminIdClaim = User.FindFirst("AdminId")?.Value;
            if (!string.IsNullOrEmpty(adminIdClaim) && int.TryParse(adminIdClaim, out int adminId))
            {
                return adminId;
            }
            return null;
        }
        protected IActionResult? CheckAdminAuthOrRedirect()
        {
            if (!IsAdminAuthenticated())
            {
                return RedirectToPage("/Auth/Login");
            }
            return null;
        }
        protected void ClearAdminSession()
        {
            // No longer needed - authentication is handled by cookies
        }
    }
}
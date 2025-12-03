using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace BookHub.Presentation.Filters
{
    public class AdminAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            
            // Check if user is authenticated and has AdminId claim (from persistent cookie)
            if (user.Identity?.IsAuthenticated != true || !user.HasClaim(c => c.Type == "AdminId"))
            {
                context.Result = new RedirectToPageResult("/Auth/Login");
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}
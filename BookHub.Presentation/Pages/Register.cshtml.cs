using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.DAL;

namespace BookHub.Presentation.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserDAL _userDAL;

        public RegisterModel(IConfiguration config)
        {
            _userDAL = new UserDAL(config.GetConnectionString("DefaultConnection")!);
        }

        [BindProperty] public string Name { get; set; } = "";
        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            if (_userDAL.UserExists(Email))
            {
                ErrorMessage = "Email already registered.";
                return Page();
            }

            _userDAL.RegisterUser(Name, Email, Password);
            return RedirectToPage("/Login");
        }
    }
}

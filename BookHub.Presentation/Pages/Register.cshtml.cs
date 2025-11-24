using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;

namespace BookHub.Presentation.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserBLL _userBLL;

        public RegisterModel(IConfiguration config)
        {
            _userBLL = new UserBLL(config.GetConnectionString("DefaultConnection")!);
        }

        [BindProperty] public string Name { get; set; } = "";
        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }

        public IActionResult OnPost()
        {
            // Presentation layer validation
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Delegate business logic to BLL
            if (!_userBLL.RegisterUser(Name, Email, Password))
            {
                ErrorMessage = "Email already registered or invalid data.";
                return Page();
            }

            return RedirectToPage("/Login");
        }
    }
}

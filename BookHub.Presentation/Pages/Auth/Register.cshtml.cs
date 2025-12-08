using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
namespace BookHub.Presentation.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IUserBLL _userBLL;
        public RegisterModel(IUserBLL userBLL)
        {
            _userBLL = userBLL;
        }
        [BindProperty] public string Name { get; set; } = "";
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public DateTime? DateOfBirth { get; set; }
        [BindProperty] public string? Gender { get; set; }
        public string? ErrorMessage { get; set; }
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }
            if (!_userBLL.RegisterUser(Name, Username, Email, Password, DateOfBirth, Gender))
            {
                ErrorMessage = "Email already registered or invalid data.";
                return Page();
            }
            return RedirectToPage("/Auth/Login");
        }
    }
}

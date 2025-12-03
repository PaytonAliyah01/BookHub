using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    public class BookClubsModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        public BookClubsModel(AdminBLL adminBLL)
        {
            _adminBLL = adminBLL;
        }
        public List<dynamic> BookClubs { get; set; } = new List<dynamic>();
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                BookClubs = _adminBLL.GetAllBookClubs();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error loading book clubs: " + ex.Message;
            }
            return Page();
        }
        public IActionResult OnPostDeleteBookClub(int clubId)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                bool success = _adminBLL.DeleteBookClub(clubId);
                if (success)
                {
                    TempData["Message"] = "Book club deleted successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to delete book club.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error deleting book club: " + ex.Message;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage();
        }
    }
}
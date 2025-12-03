using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    public class BooksModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        public BooksModel(AdminBLL adminBLL)
        {
            _adminBLL = adminBLL;
        }
        public List<Book> Books { get; set; } = new();
        public string Message { get; set; } = "";
        public string MessageType { get; set; } = "";
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                Books = _adminBLL.GetAllBooks();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Error loading books: {ex.Message}";
            }
            return Page();
        }
        public IActionResult OnPostDeleteBook(int bookId)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                bool deleted = _adminBLL.DeleteBook(bookId);
                if (deleted)
                {
                    TempData["Message"] = "Book deleted successfully!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to delete book.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error deleting book: {ex.Message}";
                TempData["MessageType"] = "error";
            }
            return RedirectToPage();
        }
    }
}
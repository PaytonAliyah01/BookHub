using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    public class BooksManagementModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        private readonly IBookBLL _bookBLL;
        public BooksManagementModel(AdminBLL adminBLL, IBookBLL bookBLL)
        {
            _adminBLL = adminBLL;
            _bookBLL = bookBLL;
        }
        public List<BookDto> Books { get; set; } = new List<BookDto>();
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                Books = _bookBLL.GetAllBooks();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error loading books: " + ex.Message;
            }
            return Page();
        }
        public IActionResult OnPostAddBook(string title, string author, string isbn, string genre, string description, string coverUrl)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                {
                    TempData["Message"] = "Title and Author are required.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage();
                }
                bool success = _adminBLL.AddBook(title, author, isbn, genre, description, coverUrl);
                if (success)
                {
                    TempData["Message"] = "Book added successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to add book.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error adding book: " + ex.Message;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage();
        }
        public IActionResult OnPostUpdateBook(int bookId, string title, string author, string isbn, string genre, string description, string coverUrl)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                {
                    TempData["Message"] = "Title and Author are required.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage();
                }
                bool success = _adminBLL.UpdateBook(bookId, title, author, isbn, genre, description, coverUrl);
                if (success)
                {
                    TempData["Message"] = "Book updated successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to update book.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error updating book: " + ex.Message;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage();
        }
        public IActionResult OnPostDeleteBook(int bookId)
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                bool success = _adminBLL.DeleteBook(bookId);
                if (success)
                {
                    TempData["Message"] = "Book deleted successfully.";
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
                TempData["Message"] = "Error deleting book: " + ex.Message;
                TempData["MessageType"] = "error";
            }
            return RedirectToPage();
        }
    }
}
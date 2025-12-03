using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.Presentation.Filters;
namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    public class EditBookModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        private readonly IWebHostEnvironment _environment;
        public EditBookModel(AdminBLL adminBLL, IWebHostEnvironment environment)
        {
            _adminBLL = adminBLL;
            _environment = environment;
        }
        [BindProperty(SupportsGet = true)]
        public int BookId { get; set; }
        [BindProperty]
        public Book Book { get; set; } = new();
        [BindProperty]
        public IFormFile? CoverImageFile { get; set; }
        public string ErrorMessage { get; set; } = "";
        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            try
            {
                var books = _adminBLL.GetAllBooks();
                var book = books.FirstOrDefault(b => b.BookId == BookId);
                if (book == null)
                {
                    ErrorMessage = "Book not found.";
                    return Page();
                }
                Book = book;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading book: {ex.Message}";
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;
            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                if (string.IsNullOrWhiteSpace(Book.Title))
                {
                    ErrorMessage = "Book title is required.";
                    return Page();
                }
                if (string.IsNullOrWhiteSpace(Book.Author))
                {
                    ErrorMessage = "Book author is required.";
                    return Page();
                }
                string coverUrl = Book.CoverUrl?.Trim() ?? "";
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var extension = Path.GetExtension(CoverImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ErrorMessage = "Please upload a valid image file (jpg, jpeg, png, gif, bmp, webp).";
                        return Page();
                    }
                    if (CoverImageFile.Length > 10 * 1024 * 1024)
                    {
                        ErrorMessage = "Cover image must be smaller than 10MB.";
                        return Page();
                    }
                    var fileName = $"book_{BookId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                    var uploadsPath = Path.Combine(_environment.WebRootPath, "images", "covers");
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }
                    var filePath = Path.Combine(uploadsPath, fileName);
                    if (!string.IsNullOrEmpty(Book.CoverUrl) && Book.CoverUrl.StartsWith("/images/covers/"))
                    {
                        var oldFileName = Path.GetFileName(Book.CoverUrl);
                        var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch
                            {
                            }
                        }
                    }
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CoverImageFile.CopyToAsync(stream);
                    }
                    coverUrl = $"/images/covers/{fileName}";
                }
                bool success = _adminBLL.UpdateBook(
                    BookId,
                    Book.Title.Trim(),
                    Book.Author.Trim(),
                    Book.ISBN?.Trim() ?? "",
                    Book.Genre?.Trim() ?? "",
                    Book.Description?.Trim() ?? "",
                    coverUrl
                );
                if (success)
                {
                    TempData["Message"] = "Book updated successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToPage("/Admin/Books");
                }
                else
                {
                    ErrorMessage = "Failed to update book. Please try again.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating book: {ex.Message}";
                return Page();
            }
        }
    }
}

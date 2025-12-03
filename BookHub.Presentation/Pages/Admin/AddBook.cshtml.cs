using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.Presentation.Filters;

namespace BookHub.Presentation.Pages.Admin
{
    [AdminAuthorization]
    public class AddBookModel : AdminBasePage
    {
        private readonly AdminBLL _adminBLL;
        private readonly IWebHostEnvironment _environment;

        public AddBookModel(AdminBLL adminBLL, IWebHostEnvironment environment)
        {
            _adminBLL = adminBLL;
            _environment = environment;
        }

        [BindProperty]
        public string Title { get; set; } = "";

        [BindProperty]
        public string Author { get; set; } = "";

        [BindProperty]
        public string? ISBN { get; set; }

        [BindProperty]
        public string? Genre { get; set; }

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        public string? CoverUrl { get; set; }

        [BindProperty]
        public IFormFile? CoverImage { get; set; }

        public string ErrorMessage { get; set; } = "";

        public IActionResult OnGet()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var authCheck = CheckAdminAuthOrRedirect();
            if (authCheck != null) return authCheck;

            if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Author))
            {
                ErrorMessage = "Title and Author are required.";
                return Page();
            }

            try
            {
                string finalCoverUrl = CoverUrl ?? "";

                // Handle image upload
                if (CoverImage != null && CoverImage.Length > 0)
                {
                    // Validate file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                    var extension = Path.GetExtension(CoverImage.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ErrorMessage = "Invalid file type. Please upload an image file.";
                        return Page();
                    }

                    if (CoverImage.Length > 10 * 1024 * 1024) // 10MB
                    {
                        ErrorMessage = "File size must be less than 10MB.";
                        return Page();
                    }

                    // Save the file
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "covers");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await CoverImage.CopyToAsync(stream);
                    }

                    finalCoverUrl = $"/images/covers/{uniqueFileName}";
                }

                bool success = _adminBLL.AddBook(
                    Title,
                    Author,
                    ISBN ?? "",
                    Genre ?? "",
                    Description ?? "",
                    finalCoverUrl
                );

                if (success)
                {
                    TempData["Message"] = "Book added successfully!";
                    TempData["MessageType"] = "success";
                    return RedirectToPage("/Admin/Books");
                }
                else
                {
                    ErrorMessage = "Failed to add book.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding book: {ex.Message}";
                return Page();
            }
        }
    }
}

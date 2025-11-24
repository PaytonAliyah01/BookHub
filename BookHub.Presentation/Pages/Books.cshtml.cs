using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;

namespace BookHub.Presentation.Pages
{
    public class BooksModel : PageModel
    {
        private readonly BookBLL _bookBLL;
        private readonly UserBookshelfBLL _userBookshelfBLL;
        private readonly BookReviewBLL _reviewBLL;
        private readonly ReadingGoalBLL _readingGoalBLL;

        public BooksModel(IConfiguration config)
        {
            _bookBLL = new BookBLL(config.GetConnectionString("DefaultConnection")!);
            _userBookshelfBLL = new UserBookshelfBLL(config.GetConnectionString("DefaultConnection")!);
            _reviewBLL = new BookReviewBLL(config.GetConnectionString("DefaultConnection")!);
            _readingGoalBLL = new ReadingGoalBLL(config.GetConnectionString("DefaultConnection")!);
        }

        public List<BookHub.DAL.Book> Books { get; set; } = new List<BookHub.DAL.Book>();
        public Dictionary<int, (double averageRating, int reviewCount)> BookRatings { get; set; } = new Dictionary<int, (double, int)>();
        public BookHub.DAL.ReadingGoal? CurrentGoal { get; set; }
        
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;

        public void OnGet()
        {
            LoadBooks();
            LoadReadingGoal();
        }

        public IActionResult OnPostAddToBookshelf(int bookId, string ownershipType = "Physical")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "Please log in to add books to your bookshelf", requireLogin = true });
                }

                // Check if book exists
                var book = _bookBLL.GetBookById(bookId);
                if (book == null)
                {
                    return new JsonResult(new { success = false, message = "Book not found" });
                }

                // Add to user's personal bookshelf with specified ownership type
                bool addedToBookshelf = _userBookshelfBLL.AddBookToUserBookshelf(userId, bookId, "Want to Read", ownershipType);
                
                if (addedToBookshelf)
                {
                    return new JsonResult(new { success = true, message = $"'{book.Title}' has been added to your bookshelf!" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = $"'{book.Title}' is already in your bookshelf!" });
                }
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An error occurred while adding the book to your bookshelf" });
            }
        }

        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public IActionResult OnGetBookDetails(int bookId)
        {
            try
            {
                // Get book details
                var book = _bookBLL.GetBookById(bookId);
                if (book == null)
                {
                    return new JsonResult(new { success = false, message = "Book not found" });
                }

                var bookDetails = new
                {
                    success = true,
                    book = new
                    {
                        book.BookId,
                        book.Title,
                        book.Author,
                        book.Genre,
                        book.ISBN,
                        book.Description,
                        book.CoverUrl,
                        book.CreatedDate
                    }
                };

                return new JsonResult(bookDetails);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error fetching book details: {ex.Message}" });
            }
        }

        private void LoadBooks()
        {
            try
            {
                Books = _bookBLL.GetAllBooks();
                
                // Load rating data for all books
                foreach (var book in Books)
                {
                    var ratingStats = _reviewBLL.GetBookRatingStats(book.BookId);
                    BookRatings[book.BookId] = ratingStats;
                }
            }
            catch (Exception)
            {
                Books = new List<BookHub.DAL.Book>();
                BookRatings = new Dictionary<int, (double, int)>();
            }
        }

        private void LoadReadingGoal()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId > 0)
                {
                    CurrentGoal = _readingGoalBLL.GetCurrentYearGoal(userId);
                }
            }
            catch
            {
                // Ignore errors when loading reading goal
            }
        }

        public IActionResult OnPostMarkAsRead(int bookId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "Please log in to mark books as read", requireLogin = true });
                }

                // Add to bookshelf as "Read"
                bool addedToBookshelf = _userBookshelfBLL.AddBookToUserBookshelf(userId, bookId, "Read", "Physical");
                
                if (addedToBookshelf)
                {
                    // Update reading goal progress
                    string progressResult = _readingGoalBLL.IncrementProgress(userId, DateTime.Now.Year);
                    
                    var book = _bookBLL.GetBookById(bookId);
                    string bookTitle = book?.Title ?? "Unknown Book";
                    
                    if (progressResult == "Success")
                    {
                        var updatedGoal = _readingGoalBLL.GetCurrentYearGoal(userId);
                        string message = $"'{bookTitle}' marked as read! Reading progress updated: {updatedGoal?.BooksRead}/{updatedGoal?.TargetBooks} books";
                        
                        return new JsonResult(new { 
                            success = true, 
                            message = message,
                            progressUpdated = true,
                            booksRead = updatedGoal?.BooksRead ?? 0,
                            targetBooks = updatedGoal?.TargetBooks ?? 0
                        });
                    }
                    else
                    {
                        return new JsonResult(new { 
                            success = true, 
                            message = $"'{bookTitle}' marked as read!",
                            progressUpdated = false
                        });
                    }
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to mark book as read" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostDownloadCoversAsync()
        {
            try
            {
                var result = await _bookBLL.DownloadMissingCoversAsync();
                return new JsonResult(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error downloading covers: {ex.Message}" });
            }
        }
    }
}

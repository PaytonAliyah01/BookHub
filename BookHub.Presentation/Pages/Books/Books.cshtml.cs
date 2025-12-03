using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    public class BooksModel : PageModel
    {
        private readonly IBookBLL _bookBLL;
        private readonly IUserBookshelfBLL _userBookshelfBLL;
        private readonly IBookReviewBLL _reviewBLL;
        private readonly IReadingGoalBLL _readingGoalBLL;
        public BooksModel(IBookBLL bookBLL, IUserBookshelfBLL userBookshelfBLL, IBookReviewBLL reviewBLL, IReadingGoalBLL readingGoalBLL)
        {
            _bookBLL = bookBLL;
            _userBookshelfBLL = userBookshelfBLL;
            _reviewBLL = reviewBLL;
            _readingGoalBLL = readingGoalBLL;
        }
        public List<BookDto> Books { get; set; } = new List<BookDto>();
        public Dictionary<int, (double averageRating, int reviewCount)> BookRatings { get; set; } = new Dictionary<int, (double, int)>();
        public ReadingGoalDto? CurrentGoal { get; set; }
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
                var book = _bookBLL.GetBookById(bookId);
                if (book == null)
                {
                    return new JsonResult(new { success = false, message = "Book not found" });
                }
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
                foreach (var book in Books)
                {
                    var ratingStats = _reviewBLL.GetBookRatingStats(book.BookId);
                    BookRatings[book.BookId] = ratingStats;
                }
            }
            catch (Exception)
            {
                Books = new List<BookDto>();
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
                bool addedToBookshelf = _userBookshelfBLL.AddBookToUserBookshelf(userId, bookId, "Read", "Physical");
                if (addedToBookshelf)
                {
                    var book = _bookBLL.GetBookById(bookId);
                    string bookTitle = book?.Title ?? "Unknown Book";
                    string progressResult = _readingGoalBLL.IncrementProgress(userId, DateTime.Now.Year);
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
    }
}

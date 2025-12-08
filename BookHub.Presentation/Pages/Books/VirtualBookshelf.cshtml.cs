using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    [Authorize] 
    public class VirtualBookshelfModel : PageModel
    {
        private readonly IUserBookshelfBLL _userBookshelfBLL;
        private readonly IBookBLL _bookBLL;
        public VirtualBookshelfModel(IUserBookshelfBLL userBookshelfBLL, IBookBLL bookBLL)
        {
            _userBookshelfBLL = userBookshelfBLL;
            _bookBLL = bookBLL;
        }
        public List<UserBookshelf> UserBooks { get; set; } = new List<UserBookshelf>();
        public Dictionary<string, int> StatusStats { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> GenreStats { get; set; } = new Dictionary<string, int>();
        public int TotalBooks { get; set; }
        public string CurrentFilter { get; set; } = "All";
        public void OnGet(string filter = "All")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    UserBooks = new List<UserBookshelf>();
                    TempData["ErrorMessage"] = "Unable to retrieve user information. Please log in again.";
                    return;
                }
                CurrentFilter = filter;
                var allUserBooks = _userBookshelfBLL.GetUserBookshelf(userId);
                UserBooks = filter switch
                {
                    "Want to Read" => allUserBooks.Where(ub => ub.Status == "Want to Read").ToList(),
                    "Reading" => allUserBooks.Where(ub => ub.Status == "Reading").ToList(),
                    "Read" => allUserBooks.Where(ub => ub.Status == "Read").ToList(),
                    _ => allUserBooks 
                };
                TotalBooks = UserBooks.Count;
                StatusStats = _userBookshelfBLL.GetUserBookshelfStats(userId);
                GenreStats = _userBookshelfBLL.GetUserGenreStats(userId);
            }
            catch (Exception ex)
            {
                UserBooks = new List<UserBookshelf>();
                TotalBooks = 0;
                TempData["ErrorMessage"] = $"An error occurred while loading your bookshelf: {ex.Message}";
            }
        }
        public IActionResult OnPostUpdateStatus(int bookId, string newStatus)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }
                bool updated = _userBookshelfBLL.UpdateBookStatusWithDates(userId, bookId, newStatus);
                if (updated)
                {
                    return new JsonResult(new { success = true, message = $"Book status updated to {newStatus}" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update book status" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error updating status: {ex.Message}" });
            }
        }
        public IActionResult OnPostUpdateStatusWithDates(int bookId, string newStatus, DateTime? dateStarted = null, DateTime? dateFinished = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }
                bool updated = _userBookshelfBLL.UpdateBookStatusWithDates(userId, bookId, newStatus, dateStarted, dateFinished);
                if (updated)
                {
                    return new JsonResult(new { success = true, message = $"Book status updated to {newStatus} with dates" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update book status with dates" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error updating status with dates: {ex.Message}" });
            }
        }
        public IActionResult OnPostUpdateReadingProgress(int bookId, int? currentPage = null, decimal? readingProgress = null, int? totalPages = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }
                bool updated = _userBookshelfBLL.UpdateReadingProgress(userId, bookId, currentPage, readingProgress, totalPages);
                if (updated)
                {
                    return new JsonResult(new { success = true, message = "Reading progress updated successfully" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update reading progress" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error updating reading progress: {ex.Message}" });
            }
        }
        public IActionResult OnPostUpdateOwnership(int bookId, string newOwnershipType)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }
                bool updated = _userBookshelfBLL.UpdateOwnershipType(userId, bookId, newOwnershipType);
                if (updated)
                {
                    return new JsonResult(new { success = true, message = $"Ownership type updated to {newOwnershipType}" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update ownership type" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error updating ownership type: {ex.Message}" });
            }
        }
        public IActionResult OnPostRemoveFromBookshelf(int bookId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }
                bool removed = _userBookshelfBLL.RemoveBookFromUserBookshelf(userId, bookId);
                if (removed)
                {
                    return new JsonResult(new { success = true, message = "Book removed from your bookshelf" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to remove book from bookshelf" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error removing book: {ex.Message}" });
            }
        }
        public IActionResult OnGetBookDetails(int bookId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }
                var book = _bookBLL.GetBookById(bookId);
                if (book == null)
                {
                    return new JsonResult(new { success = false, message = "Book not found" });
                }
                var userBooks = _userBookshelfBLL.GetUserBookshelf(userId);
                var userBook = userBooks.FirstOrDefault(ub => ub.BookId == bookId);
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
                    },
                    userBook = userBook != null ? new
                    {
                        userBook.Status,
                        userBook.OwnershipType,
                        userBook.Rating,
                        userBook.Notes,
                        userBook.DateAdded
                    } : null
                };
                return new JsonResult(bookDetails);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error fetching book details: {ex.Message}" });
            }
        }
        public JsonResult OnPostUpdateNotesAndRating(int bookId, string notes = "", int? rating = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return new JsonResult(new { success = false, message = "User not found" });
                }
                if (notes.Length > 1000)
                {
                    return new JsonResult(new { success = false, message = "Notes cannot exceed 1000 characters" });
                }
                if (rating.HasValue && (rating.Value < 1 || rating.Value > 5))
                {
                    return new JsonResult(new { success = false, message = "Rating must be between 1 and 5 stars" });
                }
                var currentBook = _userBookshelfBLL.GetUserBookshelf(userId)
                    .FirstOrDefault(b => b.BookId == bookId);
                if (currentBook == null)
                {
                    return new JsonResult(new { success = false, message = "Book not found in your bookshelf" });
                }
                bool success = _userBookshelfBLL.UpdateBookStatus(userId, bookId, currentBook.Status, rating, notes);
                if (success)
                {
                    string message = "Notes";
                    if (rating.HasValue)
                    {
                        message = "Notes and rating";
                    }
                    return new JsonResult(new { success = true, message = $"{message} updated successfully!" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to update notes and rating" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error updating notes and rating: {ex.Message}" });
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
    }
}

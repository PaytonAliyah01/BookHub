using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;

namespace BookHub.Presentation.Pages
{
    [Authorize] // Require authentication to access bookshelf
    public class VirtualBookshelfModel : PageModel
    {
        private readonly UserBookshelfBLL _userBookshelfBLL;
        private readonly BookBLL _bookBLL;

        public VirtualBookshelfModel(IConfiguration config)
        {
            _userBookshelfBLL = new UserBookshelfBLL(config.GetConnectionString("DefaultConnection")!);
            _bookBLL = new BookBLL(config.GetConnectionString("DefaultConnection")!);
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
                    // User not authenticated properly
                    UserBooks = new List<UserBookshelf>();
                    return;
                }

                CurrentFilter = filter;

                // Get user's complete bookshelf
                var allUserBooks = _userBookshelfBLL.GetUserBookshelf(userId);
                
                // Filter books based on status
                UserBooks = filter switch
                {
                    "Want to Read" => allUserBooks.Where(ub => ub.Status == "Want to Read").ToList(),
                    "Reading" => allUserBooks.Where(ub => ub.Status == "Reading").ToList(),
                    "Read" => allUserBooks.Where(ub => ub.Status == "Read").ToList(),
                    _ => allUserBooks // "All" or any other value
                };

                TotalBooks = UserBooks.Count;

                // Calculate statistics
                StatusStats = _userBookshelfBLL.GetUserBookshelfStats(userId);
                GenreStats = _userBookshelfBLL.GetUserGenreStats(userId);
            }
            catch (Exception)
            {
                // Handle error gracefully
                UserBooks = new List<UserBookshelf>();
                TotalBooks = 0;
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

                bool updated = _userBookshelfBLL.UpdateBookStatus(userId, bookId, newStatus);
                
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

                // Get book details
                var book = _bookBLL.GetBookById(bookId);
                if (book == null)
                {
                    return new JsonResult(new { success = false, message = "Book not found" });
                }

                // Get user's specific data for this book
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;
using BookHub.DAL;

namespace BookHub.Presentation.Pages.Books
{
    [Authorize]
    public class BookDetailsModel : PageModel
    {
        private readonly IBookBLL _bookBLL;
        private readonly IUserBookshelfBLL _userBookshelfBLL;
        private readonly IBookReviewBLL _bookReviewBLL;

        public BookDetailsModel(IBookBLL bookBLL, IUserBookshelfBLL userBookshelfBLL, IBookReviewBLL bookReviewBLL)
        {
            _bookBLL = bookBLL;
            _userBookshelfBLL = userBookshelfBLL;
            _bookReviewBLL = bookReviewBLL;
        }

        [BindProperty(SupportsGet = true)]
        public int BookId { get; set; }

        public BookDto? Book { get; set; }
        public List<BookReviewDto> Reviews { get; set; } = new();
        public bool IsInBookshelf { get; set; }
        public decimal AverageRating { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                Book = _bookBLL.GetBookById(BookId);
                if (Book == null)
                {
                    return Page();
                }

                Reviews = _bookReviewBLL.GetReviewsForBook(BookId);
                
                if (Reviews.Any())
                {
                    AverageRating = (decimal)Reviews.Average(r => r.Rating);
                }

                if (User.Identity?.IsAuthenticated == true && !User.HasClaim(c => c.Type == "AdminId"))
                {
                    var userId = GetCurrentUserId();
                    if (userId > 0)
                    {
                        var userBooks = _userBookshelfBLL.GetUserBookshelf(userId);
                        IsInBookshelf = userBooks.Any(ub => ub.BookId == BookId);
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error loading book details: {ex.Message}";
                TempData["MessageType"] = "error";
                return RedirectToPage("/Books");
            }
        }

        public IActionResult OnPostAddToBookshelf(int bookId, string status, string ownershipType)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                bool success = _userBookshelfBLL.AddBookToUserBookshelf(userId, bookId, status, ownershipType);
                
                if (success)
                {
                    TempData["Message"] = "Book added to your bookshelf!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to add book to bookshelf.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { bookId });
        }

        public IActionResult OnPostRemoveFromBookshelf(int bookId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                bool success = _userBookshelfBLL.RemoveBookFromUserBookshelf(userId, bookId);
                
                if (success)
                {
                    TempData["Message"] = "Book removed from your bookshelf.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to remove book.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { bookId });
        }

        public IActionResult OnPostWriteReview(int bookId, int rating, string reviewText)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var review = new BookReviewDto
                {
                    UserId = userId,
                    BookId = bookId,
                    Rating = rating,
                    ReviewText = reviewText,
                    ReviewDate = DateTime.Now
                };

                bool success = _bookReviewBLL.AddReview(review);
                
                if (success)
                {
                    TempData["Message"] = "Review submitted successfully!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to submit review.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { bookId });
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;

namespace BookHub.Presentation.Pages
{
    public class BookReviewsModel : PageModel
    {
        private readonly BookBLL _bookBLL;
        private readonly BookReviewBLL _reviewBLL;
        private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BookHubDb;Integrated Security=True;";

        [BindProperty]
        public BookReview NewReview { get; set; } = new BookReview();

        public Book? Book { get; set; }
        public List<BookReview> Reviews { get; set; } = new List<BookReview>();
        public BookReview? UserReview { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        
        [BindProperty]
        public int BookId { get; set; }

        public BookReviewsModel()
        {
            _bookBLL = new BookBLL(_connectionString);
            _reviewBLL = new BookReviewBLL(_connectionString);
        }

        public IActionResult OnGet(int bookId)
        {
            BookId = bookId;

            // Get book details
            Book = _bookBLL.GetBookById(bookId);
            if (Book == null)
            {
                return NotFound();
            }

            // Get current user ID from claims
            var userId = GetCurrentUserId();
            var userIdNullable = userId > 0 ? (int?)userId : null;

            // Get all reviews for this book
            Reviews = _reviewBLL.GetReviewsForBook(bookId);

            // Get user's existing review if logged in
            if (userIdNullable.HasValue)
            {
                UserReview = _reviewBLL.GetUserReviewForBook(userIdNullable.Value, bookId);
            }

            // Get rating statistics
            var stats = _reviewBLL.GetBookRatingStats(bookId);
            AverageRating = stats.averageRating;
            ReviewCount = stats.reviewCount;

            return Page();
        }

        public IActionResult OnPostAddReview()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to write a review.";
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                // Reload page data
                return OnGet(BookId);
            }

            // Set review properties
            NewReview.BookId = BookId;
            NewReview.UserId = userId;
            NewReview.ReviewDate = DateTime.Now;

            if (_reviewBLL.AddReview(NewReview))
            {
                TempData["SuccessMessage"] = "Review added successfully!";
                return RedirectToPage("/BookReviews", new { bookId = BookId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add review. Please try again.";
                return OnGet(BookId);
            }
        }

        public IActionResult OnPostUpdateReview()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                return OnGet(BookId);
            }

            NewReview.UserId = userId;
            NewReview.BookId = BookId;
            NewReview.LastModified = DateTime.Now;

            if (_reviewBLL.UpdateReview(NewReview))
            {
                TempData["SuccessMessage"] = "Review updated successfully!";
                return RedirectToPage("/BookReviews", new { bookId = BookId });
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update review. Please try again.";
                return OnGet(BookId);
            }
        }

        public IActionResult OnPostDeleteReview(int reviewId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToPage("/Login");
            }

            if (_reviewBLL.DeleteReview(reviewId, userId))
            {
                TempData["SuccessMessage"] = "Review deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete review. Please try again.";
            }

            return RedirectToPage("/BookReviews", new { bookId = BookId });
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
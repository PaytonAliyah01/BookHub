using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    public class BookReviewsModel : PageModel
    {
        private readonly IBookBLL _bookBLL;
        private readonly IBookReviewBLL _reviewBLL;
        [BindProperty]
        public BookReviewDto NewReview { get; set; } = new BookReviewDto();
        public BookDto? Book { get; set; }
        public List<BookReviewDto> Reviews { get; set; } = new List<BookReviewDto>();
        public BookReviewDto? UserReview { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        [BindProperty]
        public int BookId { get; set; }
        public BookReviewsModel(IBookBLL bookBLL, IBookReviewBLL reviewBLL)
        {
            _bookBLL = bookBLL;
            _reviewBLL = reviewBLL;
        }
        public IActionResult OnGet(int bookId)
        {
            BookId = bookId;
            Book = _bookBLL.GetBookById(bookId);
            if (Book == null)
            {
                return NotFound();
            }
            var userId = GetCurrentUserId();
            var userIdNullable = userId > 0 ? (int?)userId : null;
            Reviews = _reviewBLL.GetReviewsForBook(bookId);
            if (userIdNullable.HasValue)
            {
                UserReview = _reviewBLL.GetUserReviewForBook(userIdNullable.Value, bookId);
            }
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
                return RedirectToPage("/Auth/Login");
            }
            if (!ModelState.IsValid)
            {
                return OnGet(BookId);
            }
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
                return RedirectToPage("/Auth/Login");
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
                return RedirectToPage("/Auth/Login");
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

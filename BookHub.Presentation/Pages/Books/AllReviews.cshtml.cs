using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    public class AllReviewsModel : PageModel
    {
        private readonly IBookReviewBLL _reviewBLL;
        private readonly IBookBLL _bookBLL;
        public List<BookReviewDto> AllReviews { get; set; } = new List<BookReviewDto>();
        public string SearchQuery { get; set; } = string.Empty;
        public string SortBy { get; set; } = "recent";
        public int? FilterRating { get; set; }
        public string FilterGenre { get; set; } = string.Empty;
        public bool ShowMyReviews { get; set; } = false;
        public List<string> AvailableGenres { get; set; } = new List<string>();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalReviews { get; set; }
        private const int PageSize = 12;
        public AllReviewsModel(IBookReviewBLL reviewBLL, IBookBLL bookBLL)
        {
            _reviewBLL = reviewBLL;
            _bookBLL = bookBLL;
        }
        public void OnGet(string search = "", string sort = "recent", int? rating = null, string genre = "", int page = 1, bool myreviews = false)
        {
            SearchQuery = search ?? "";
            SortBy = sort ?? "recent";
            FilterRating = rating;
            FilterGenre = genre ?? "";
            ShowMyReviews = myreviews;
            CurrentPage = Math.Max(1, page);
            LoadReviews();
            LoadGenres();
        }
        private void LoadReviews()
        {
            try
            {
                var allReviews = new List<BookReviewDto>();
                var books = _bookBLL.GetAllBooks();
                foreach (var book in books)
                {
                    var bookReviews = _reviewBLL.GetReviewsForBook(book.BookId);
                    foreach (var review in bookReviews)
                    {
                        review.BookTitle = book.Title;
                        review.BookAuthor = book.Author;
                        review.BookGenre = book.Genre;
                    }
                    allReviews.AddRange(bookReviews);
                }
                var filteredReviews = allReviews.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var query = SearchQuery.ToLower();
                    filteredReviews = filteredReviews.Where(r =>
                        (r.BookTitle?.ToLower().Contains(query) ?? false) ||
                        (r.BookAuthor?.ToLower().Contains(query) ?? false) ||
                        r.ReviewTitle.ToLower().Contains(query) ||
                        r.ReviewText.ToLower().Contains(query) ||
                        (r.Username?.ToLower().Contains(query) ?? false)
                    );
                }
                if (FilterRating.HasValue)
                {
                    filteredReviews = filteredReviews.Where(r => r.Rating == FilterRating.Value);
                }
                if (!string.IsNullOrWhiteSpace(FilterGenre))
                {
                    filteredReviews = filteredReviews.Where(r =>
                        !string.IsNullOrEmpty(r.BookGenre) &&
                        r.BookGenre.Equals(FilterGenre, StringComparison.OrdinalIgnoreCase)
                    );
                }
                if (ShowMyReviews)
                {
                    var currentUserId = GetCurrentUserId();
                    if (currentUserId > 0)
                    {
                        filteredReviews = filteredReviews.Where(r => r.UserId == currentUserId);
                    }
                    else
                    {
                        filteredReviews = Enumerable.Empty<BookReviewDto>();
                    }
                }
                filteredReviews = SortBy switch
                {
                    "oldest" => filteredReviews.OrderBy(r => r.ReviewDate),
                    "rating_high" => filteredReviews.OrderByDescending(r => r.Rating).ThenByDescending(r => r.ReviewDate),
                    "rating_low" => filteredReviews.OrderBy(r => r.Rating).ThenByDescending(r => r.ReviewDate),
                    "title" => filteredReviews.OrderBy(r => r.BookTitle ?? string.Empty).ThenByDescending(r => r.ReviewDate),
                    "author" => filteredReviews.OrderBy(r => r.BookAuthor ?? string.Empty).ThenByDescending(r => r.ReviewDate),
                    _ => filteredReviews.OrderByDescending(r => r.ReviewDate) 
                };
                var reviewsList = filteredReviews.ToList();
                TotalReviews = reviewsList.Count;
                TotalPages = (int)Math.Ceiling((double)TotalReviews / PageSize);
                AllReviews = reviewsList
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            catch (Exception)
            {
                AllReviews = new List<BookReviewDto>();
                TotalReviews = 0;
                TotalPages = 0;
            }
        }
        private void LoadGenres()
        {
            try
            {
                var books = _bookBLL.GetAllBooks();
                AvailableGenres = books
                    .Where(b => !string.IsNullOrWhiteSpace(b.Genre))
                    .Select(b => b.Genre)
                    .Distinct()
                    .OrderBy(g => g)
                    .ToList();
            }
            catch (Exception)
            {
                AvailableGenres = new List<string>();
            }
        }
        public string GetPaginationUrl(int page)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(SearchQuery))
                queryParams.Add($"search={Uri.EscapeDataString(SearchQuery)}");
            if (SortBy != "recent")
                queryParams.Add($"sort={SortBy}");
            if (FilterRating.HasValue)
                queryParams.Add($"rating={FilterRating}");
            if (!string.IsNullOrWhiteSpace(FilterGenre))
                queryParams.Add($"genre={Uri.EscapeDataString(FilterGenre)}");
            if (ShowMyReviews)
                queryParams.Add("myreviews=true");
            if (page > 1)
                queryParams.Add($"page={page}");
            return queryParams.Count > 0 ? $"/AllReviews?{string.Join("&", queryParams)}" : "/AllReviews";
        }
        public IActionResult OnPostDeleteReview(int reviewId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to delete reviews.";
                return RedirectToPage();
            }
            if (_reviewBLL.DeleteReview(reviewId, userId))
            {
                TempData["SuccessMessage"] = "Review deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete review. You can only delete your own reviews.";
            }
            return RedirectToPage();
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

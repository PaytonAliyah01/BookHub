using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    public class ReadingProgressModel : PageModel
    {
        private readonly IUserBookshelfBLL _userBookshelfBLL;
        private readonly UserDAL _userDAL;
        public ReadingProgressModel(IUserBookshelfBLL userBookshelfBLL, IConfiguration configuration)
        {
            _userBookshelfBLL = userBookshelfBLL;
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _userDAL = new UserDAL(connectionString);
        }
        public List<UserBookshelf> CurrentlyReadingBooks { get; set; } = new();
        public List<UserBookshelf> RecentlyFinishedBooks { get; set; } = new();
        public string Message { get; set; } = "";
        public User? CurrentUser { get; set; }
        public void OnGet()
        {
            LoadData();
        }
        public IActionResult OnPostUpdateStatus(int bookId, string newStatus, string? dateStarted, string? dateFinished)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            DateTime? startDate = null;
            DateTime? endDate = null;
            if (!string.IsNullOrEmpty(dateStarted) && DateTime.TryParse(dateStarted, out DateTime start))
            {
                startDate = start;
            }
            if (!string.IsNullOrEmpty(dateFinished) && DateTime.TryParse(dateFinished, out DateTime end))
            {
                endDate = end;
            }
            bool success = _userBookshelfBLL.UpdateBookStatusWithDates(currentUser.UserId, bookId, newStatus, startDate, endDate);
            Message = success ? "Book status updated successfully!" : "Failed to update book status.";
            LoadData();
            return Page();
        }
        public IActionResult OnPostUpdateProgress(int bookId, int? currentPage, decimal? readingProgress, int? totalPages)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            bool success = _userBookshelfBLL.UpdateReadingProgress(currentUser.UserId, bookId, currentPage, readingProgress, totalPages);
            Message = success ? "Reading progress updated successfully!" : "Failed to update reading progress.";
            LoadData();
            return Page();
        }
        private void LoadData()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return;
            CurrentUser = _userDAL.GetUserById(currentUser.UserId);
            if (CurrentUser == null) return;
            var allBooks = _userBookshelfBLL.GetUserBookshelf(CurrentUser.UserId);
            CurrentlyReadingBooks = allBooks
                .Where(ub => ub.Status == "Reading")
                .OrderByDescending(ub => ub.DateAdded)
                .ToList();
            RecentlyFinishedBooks = allBooks
                .Where(ub => ub.Status == "Read")
                .OrderByDescending(ub => ub.DateFinished ?? ub.DateAdded)
                .Take(10)
                .ToList();
        }
        private UserDto? GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return new UserDto { UserId = userId };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

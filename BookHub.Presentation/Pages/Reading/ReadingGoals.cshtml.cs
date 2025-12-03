using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    public class ReadingGoalsModel : PageModel
    {
        private readonly IReadingGoalBLL _readingGoalBLL;
        private readonly IUserBLL _userBLL;
        private readonly IAnalyticsBLL _analyticsBLL;
        private readonly UserDAL _userDAL;
        private readonly UserBookshelfDAL _userBookshelfDAL;
        public ReadingGoalsModel(IReadingGoalBLL readingGoalBLL, IUserBLL userBLL, IAnalyticsBLL analyticsBLL, IConfiguration configuration)
        {
            _readingGoalBLL = readingGoalBLL;
            _userBLL = userBLL;
            _analyticsBLL = analyticsBLL;
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _userDAL = new UserDAL(connectionString);
            _userBookshelfDAL = new UserBookshelfDAL(connectionString);
        }
        public ReadingGoalDto? CurrentGoal { get; set; }
        public List<ReadingGoalDto> AllGoals { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public string Message { get; set; } = "";
        public string MotivationalMessage { get; set; } = "";
        public Dictionary<string, object> ProgressAnalytics { get; set; } = new();
        public Dictionary<string, object> ReadingProgressData { get; set; } = new();
        public Dictionary<string, object> BookshelfAnalytics { get; set; } = new();
        public User? CurrentUser { get; set; }
        public List<UserBookshelf> CompletedBooksCurrentYear { get; set; } = new();
        public Dictionary<int, List<UserBookshelf>> CompletedBooksByYear { get; set; } = new();
        public void OnGet()
        {
            LoadData();
            LoadAnalyticsData();
        }
        public IActionResult OnPostSetGoal(int year, int targetBooks)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            string result = _readingGoalBLL.SetReadingGoal(currentUser.UserId, year, targetBooks);
            Message = result == "Success" 
                ? $"Reading goal set successfully for {year}!" 
                : result;
            LoadData();
            return Page();
        }
        public IActionResult OnPostUpdateProgress(int year, int booksRead)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            string result = _readingGoalBLL.UpdateProgress(currentUser.UserId, year, booksRead);
            Message = result == "Success" 
                ? "Progress updated successfully!" 
                : result;
            LoadData();
            return Page();
        }
        public IActionResult OnPostIncrementProgress(int year)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            string result = _readingGoalBLL.IncrementProgress(currentUser.UserId, year);
            Message = result == "Success" 
                ? "Great job! One more book added to your progress! 📚" 
                : result;
            LoadData();
            return Page();
        }
        public IActionResult OnPostDecrementProgress(int year)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            string result = _readingGoalBLL.DecrementProgress(currentUser.UserId, year);
            Message = result == "Success" 
                ? "Progress updated." 
                : result;
            LoadData();
            return Page();
        }
        public IActionResult OnPostDeleteGoal(int goalId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            string result = _readingGoalBLL.DeleteReadingGoal(goalId);
            Message = result == "Success" 
                ? "Reading goal deleted successfully." 
                : result;
            LoadData();
            return Page();
        }
        public IActionResult OnPostEditGoal(int year, int newTargetBooks)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Auth/Login");
            }
            string result = _readingGoalBLL.UpdateGoalTarget(currentUser.UserId, year, newTargetBooks);
            Message = result == "Success" 
                ? $"Reading goal for {year} updated successfully!" 
                : result;
            LoadData();
            return Page();
        }
        private void LoadData()
        {
            AvailableYears = _readingGoalBLL.GetAvailableYears();
            var currentUser = GetCurrentUser();
            if (currentUser == null) 
            {
                ProgressAnalytics = new Dictionary<string, object> { ["HasGoal"] = false };
                return;
            }
            CurrentGoal = _readingGoalBLL.GetCurrentYearGoal(currentUser.UserId);
            AllGoals = _readingGoalBLL.GetUserReadingGoals(currentUser.UserId);
            MotivationalMessage = _readingGoalBLL.GetMotivationalMessage(CurrentGoal);
            ProgressAnalytics = _readingGoalBLL.GetProgressAnalytics(CurrentGoal);
            LoadCompletedBooks(currentUser.UserId);
        }
        private void LoadAnalyticsData()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return;
            try
            {
                CurrentUser = _userDAL.GetUserById(currentUser.UserId);
                if (CurrentUser != null)
                {
                    ReadingProgressData = _analyticsBLL.GetReadingProgressData(CurrentUser.UserId);
                    BookshelfAnalytics = _analyticsBLL.GetBookshelfAnalytics(CurrentUser.UserId);
                }
            }
            catch
            {
                ReadingProgressData = new Dictionary<string, object> { ["Error"] = "Unable to load analytics" };
                BookshelfAnalytics = new Dictionary<string, object> { ["Error"] = "Unable to load analytics" };
            }
        }
        private void LoadCompletedBooks(int userId)
        {
            try
            {
                var allBooks = _userBookshelfDAL.GetUserBookshelf(userId);
                var completedBooks = allBooks.Where(ub => ub.Status == "Read").ToList();
                var currentYear = DateTime.Now.Year;
                CompletedBooksCurrentYear = completedBooks
                    .Where(cb => cb.DateAdded.Year == currentYear)
                    .OrderByDescending(cb => cb.DateAdded)
                    .ToList();
                CompletedBooksByYear = completedBooks
                    .GroupBy(cb => cb.DateAdded.Year)
                    .ToDictionary(
                        g => g.Key, 
                        g => g.OrderByDescending(cb => cb.DateAdded).ToList()
                    );
            }
            catch
            {
                CompletedBooksCurrentYear = new List<UserBookshelf>();
                CompletedBooksByYear = new Dictionary<int, List<UserBookshelf>>();
            }
        }
        private UserDto? GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return _userBLL.GetUserById(userId);
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

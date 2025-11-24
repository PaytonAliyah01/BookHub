using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;

namespace BookHub.Presentation.Pages
{
    public class ReadingGoalsModel : PageModel
    {
        private readonly ReadingGoalBLL _readingGoalBLL;
        private readonly UserDAL _userDAL;

        public ReadingGoalsModel(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _readingGoalBLL = new ReadingGoalBLL(connectionString);
            _userDAL = new UserDAL(connectionString);
        }

        public BookHub.DAL.ReadingGoal? CurrentGoal { get; set; }
        public List<BookHub.DAL.ReadingGoal> AllGoals { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public string Message { get; set; } = "";
        public string MotivationalMessage { get; set; } = "";
        public Dictionary<string, object> ProgressAnalytics { get; set; } = new();

        public void OnGet()
        {
            LoadData();
        }

        public IActionResult OnPostSetGoal(int year, int targetBooks)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToPage("/Login");
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
                return RedirectToPage("/Login");
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
                return RedirectToPage("/Login");
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
                return RedirectToPage("/Login");
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
                return RedirectToPage("/Login");
            }

            string result = _readingGoalBLL.DeleteReadingGoal(goalId);
            Message = result == "Success" 
                ? "Reading goal deleted successfully." 
                : result;

            LoadData();
            return Page();
        }

        private void LoadData()
        {
            // Always populate available years regardless of user login status
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
        }

        private User? GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return _userDAL.GetUserById(userId);
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
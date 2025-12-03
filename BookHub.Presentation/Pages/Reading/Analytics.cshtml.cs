using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;
using BookHub.DAL;
using System.Security.Claims;
namespace BookHub.Presentation.Pages
{
    [Authorize]
    public class AnalyticsModel : PageModel
    {
        private readonly IAnalyticsBLL _analyticsBLL;
        private readonly UserDAL _userDAL;
        public AnalyticsModel(IAnalyticsBLL analyticsBLL, IConfiguration configuration)
        {
            _analyticsBLL = analyticsBLL;
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _userDAL = new UserDAL(connectionString);
        }
        public Dictionary<string, object> ReadingProgressData { get; set; } = new();
        public Dictionary<string, object> ReviewAnalytics { get; set; } = new();
        public Dictionary<string, object> BookshelfAnalytics { get; set; } = new();
        public User? CurrentUser { get; set; }
        public void OnGet()
        {
            LoadAnalyticsData();
        }
        private void LoadAnalyticsData()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return;
            CurrentUser = currentUser;
            ReadingProgressData = _analyticsBLL.GetReadingProgressData(currentUser.UserId);
            ReviewAnalytics = _analyticsBLL.GetReviewAnalytics(currentUser.UserId);
            BookshelfAnalytics = _analyticsBLL.GetBookshelfAnalytics(currentUser.UserId);
        }
        private User? GetCurrentUser()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                    return null;
                return _userDAL.GetUserWithCredentials(email);
            }
            catch
            {
                return null;
            }
        }
    }
}

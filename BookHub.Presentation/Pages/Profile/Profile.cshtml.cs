using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BookHub.BLL;
namespace BookHub.Presentation.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IUserBLL _userBLL;
        private readonly IConfiguration _configuration;
        
        public ProfileModel(IUserBLL userBLL, IConfiguration configuration)
        {
            _userBLL = userBLL;
            _configuration = configuration;
        }
        
        public UserDto? UserProfile { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Reading statistics
        public int TotalBooksRead { get; set; }
        public int BookClubsJoined { get; set; }
        public int ReviewsWritten { get; set; }
        public int FriendsCount { get; set; }
        public IActionResult OnGet()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToPage("/Auth/Login");
                }
                UserProfile = _userBLL.GetUserProfile(email);
                if (UserProfile == null)
                {
                    ErrorMessage = "Unable to load user profile.";
                }
                else
                {
                    // Load reading statistics
                    var connectionString = _configuration.GetConnectionString("DefaultConnection");
                    LoadStatistics(UserProfile.UserId, connectionString ?? "");
                }
                return Page();
            }
            catch (ApplicationException ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
            catch (Exception)
            {
                ErrorMessage = "An unexpected error occurred while loading your profile.";
                return Page();
            }
        }
        
        private void LoadStatistics(int userId, string connectionString)
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                conn.Open();
                
                // Total books read (Status = 'Completed')
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM UserBooks WHERE UserId = @UserId AND Status = 'Completed'", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    TotalBooksRead = (int)cmd.ExecuteScalar();
                }
                
                // Book clubs joined
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM BookClubMembers WHERE UserId = @UserId AND Status = 'Accepted'", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    BookClubsJoined = (int)cmd.ExecuteScalar();
                }
                
                // Reviews written
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM BookReviews WHERE UserId = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    ReviewsWritten = (int)cmd.ExecuteScalar();
                }
                
                // Friends count
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM Friends WHERE (UserId1 = @UserId OR UserId2 = @UserId) AND Status = 'Accepted'", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    FriendsCount = (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                // Ignore errors loading statistics
                TotalBooksRead = 0;
                BookClubsJoined = 0;
                ReviewsWritten = 0;
                FriendsCount = 0;
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;

namespace BookHub.Presentation.Pages
{
    public class FriendsModel : PageModel
    {
        private readonly FriendBLL _friendBLL;
        private readonly string _connectionString;

        public FriendsModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _friendBLL = new FriendBLL(_connectionString);
        }

        [BindProperty]
        public string SearchTerm { get; set; } = string.Empty;

        public List<User> SearchResults { get; set; } = new List<User>();
        public List<User> Friends { get; set; } = new List<User>();
        public List<FriendRequest> PendingRequests { get; set; } = new List<FriendRequest>();
        public List<dynamic> FriendsActivity { get; set; } = new List<dynamic>();
        public Dictionary<int, string> UserRelationships { get; set; } = new Dictionary<int, string>();

        public string CurrentTab { get; set; } = "friends";

        public void OnGet(string tab = "friends")
        {
            CurrentTab = tab;
            LoadUserData();
        }

        public IActionResult OnPostSearch()
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                TempData["ErrorMessage"] = "You must be logged in to search for users.";
                return RedirectToPage("/Login");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    SearchResults = _friendBLL.SearchUsers(SearchTerm, userId);
                    
                    // Get relationship status for each search result
                    foreach (var user in SearchResults)
                    {
                        UserRelationships[user.UserId] = _friendBLL.GetRelationshipStatus(userId, user.UserId);
                    }
                }

                CurrentTab = "search";
                LoadUserData();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                CurrentTab = "search";
                LoadUserData();
                return Page();
            }
        }

        public IActionResult OnPostSendRequest(int toUserId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(new { success = false, message = "You must be logged in." });
            }

            try
            {
                bool success = _friendBLL.SendFriendRequest(userId, toUserId);
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Friend request sent successfully!" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Unable to send friend request. You may already be friends or have a pending request." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public IActionResult OnPostAcceptRequest(int requestId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(new { success = false, message = "You must be logged in." });
            }

            try
            {
                bool success = _friendBLL.AcceptFriendRequest(requestId, userId);
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Friend request accepted!" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Unable to accept friend request." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public IActionResult OnPostDeclineRequest(int requestId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(new { success = false, message = "You must be logged in." });
            }

            try
            {
                bool success = _friendBLL.DeclineFriendRequest(requestId, userId);
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Friend request declined." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Unable to decline friend request." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public IActionResult OnPostRemoveFriend(int friendUserId)
        {
            var userId = GetCurrentUserId();
            if (userId <= 0)
            {
                return new JsonResult(new { success = false, message = "You must be logged in." });
            }

            try
            {
                bool success = _friendBLL.RemoveFriend(userId, friendUserId);
                if (success)
                {
                    return new JsonResult(new { success = true, message = "Friend removed successfully." });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Unable to remove friend." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private void LoadUserData()
        {
            var userId = GetCurrentUserId();
            if (userId > 0)
            {
                try
                {
                    Friends = _friendBLL.GetFriends(userId);
                    PendingRequests = _friendBLL.GetPendingRequests(userId);
                    FriendsActivity = _friendBLL.GetFriendsActivity(userId, 20);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error loading friends data: {ex.Message}";
                }
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
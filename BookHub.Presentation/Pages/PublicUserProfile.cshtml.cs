using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;

namespace BookHub.Presentation.Pages
{
    [Authorize]
    public class PublicUserProfileModel : PageModel
    {
        private readonly IUserBLL _userBLL;
        private readonly IUserBookshelfBLL _userBookshelfBLL;
        private readonly IBookReviewBLL _bookReviewBLL;
        private readonly IReadingGoalBLL _readingGoalBLL;
        private readonly IFriendBLL _friendsBLL;

        public PublicUserProfileModel(
            IUserBLL userBLL, 
            IUserBookshelfBLL userBookshelfBLL, 
            IBookReviewBLL bookReviewBLL,
            IReadingGoalBLL readingGoalBLL,
            IFriendBLL friendsBLL)
        {
            _userBLL = userBLL;
            _userBookshelfBLL = userBookshelfBLL;
            _bookReviewBLL = bookReviewBLL;
            _readingGoalBLL = readingGoalBLL;
            _friendsBLL = friendsBLL;
        }

        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }

        public UserDto? ProfileUser { get; set; }
        public List<BookDto> PublicBooks { get; set; } = new();
        public List<BookReviewDto> RecentReviews { get; set; } = new();
        public ReadingGoalDto? ReadingGoal { get; set; }
        
        public int TotalBooks { get; set; }
        public int BooksRead { get; set; }
        public int BooksReading { get; set; }
        public int BooksWantToRead { get; set; }
        public double GoalProgress { get; set; }
        
        public bool IsOwnProfile { get; set; }
        public bool IsFriend { get; set; }
        public bool HasPendingRequest { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                IsOwnProfile = (UserId == currentUserId);

                if (IsOwnProfile)
                {
                    return RedirectToPage("/Profile");
                }

                ProfileUser = _userBLL.GetUserById(UserId);
                if (ProfileUser == null)
                {
                    return Page();
                }

                var userBookshelf = _userBookshelfBLL.GetUserBookshelf(UserId);
                TotalBooks = userBookshelf.Count;
                BooksRead = userBookshelf.Count(b => b.Status == "Read");
                BooksReading = userBookshelf.Count(b => b.Status == "Reading");
                BooksWantToRead = userBookshelf.Count(b => b.Status == "Want to Read");

                PublicBooks = userBookshelf
                    .Where(ub => ub.Book != null)
                    .Select(ub => new BookDto
                    {
                        BookId = ub.Book!.BookId,
                        Title = ub.Book.Title,
                        Author = ub.Book.Author,
                        ISBN = ub.Book.ISBN,
                        CoverUrl = ub.Book.CoverUrl,
                        Genre = ub.Book.Genre,
                        Description = ub.Book.Description
                    }).ToList();

                RecentReviews = new List<BookReviewDto>();

                try
                {
                    ReadingGoal = _readingGoalBLL.GetGoalByYear(UserId, DateTime.Now.Year);
                    if (ReadingGoal != null && ReadingGoal.TargetBooks > 0)
                    {
                        GoalProgress = Math.Round((double)ReadingGoal.BooksRead / ReadingGoal.TargetBooks * 100, 0);
                    }
                }
                catch { }

                try
                {
                    var status = _friendsBLL.GetRelationshipStatus(currentUserId, UserId);
                    IsFriend = (status == "Friends");
                    HasPendingRequest = (status == "Pending");
                }
                catch { }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error loading profile: {ex.Message}";
                TempData["MessageType"] = "error";
                return RedirectToPage("/Index");
            }
        }

        public IActionResult OnPostSendFriendRequest(int targetUserId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                bool success = _friendsBLL.SendFriendRequest(currentUserId, targetUserId);
                
                if (success)
                {
                    TempData["Message"] = "Friend request sent!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to send friend request.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { userId = targetUserId });
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

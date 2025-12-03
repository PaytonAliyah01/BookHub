using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;

namespace BookHub.Presentation.Pages.Community
{
    [Authorize]
    public class BookClubDetailsModel : PageModel
    {
        private readonly IBookClubBLL _bookClubBLL;
        private readonly IForumBLL _forumBLL;
        private readonly IBookBLL _bookBLL;

        public BookClubDetailsModel(IBookClubBLL bookClubBLL, IForumBLL forumBLL, IBookBLL bookBLL)
        {
            _bookClubBLL = bookClubBLL;
            _forumBLL = forumBLL;
            _bookBLL = bookBLL;
        }

        [BindProperty(SupportsGet = true)]
        public int ClubId { get; set; }

        public BookHub.DAL.BookClub? Club { get; set; }
        public List<BookHub.DAL.BookClubMember> Members { get; set; } = new();
        public List<BookHub.DAL.DiscussionPost> Discussions { get; set; } = new();
        public BookDto? CurrentBook { get; set; }
        public int MemberCount { get; set; }
        public bool IsMember { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                Club = _bookClubBLL.GetBookClubById(ClubId);
                if (Club == null)
                {
                    return Page();
                }

                Members = _bookClubBLL.GetBookClubMembers(ClubId);
                MemberCount = Members.Count;
                IsMember = Members.Any(m => m.UserId == userId);

                Discussions = _forumBLL.GetDiscussionPosts(ClubId, 1, 50);

                if (Club.CurrentBookId.HasValue && Club.CurrentBookId.Value > 0)
                {
                    CurrentBook = _bookBLL.GetBookById(Club.CurrentBookId.Value);
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error loading book club: {ex.Message}";
                TempData["MessageType"] = "error";
                return RedirectToPage("/Community/BookClubs");
            }
        }

        public IActionResult OnPostJoinClub(int clubId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                bool success = _bookClubBLL.JoinBookClub(clubId, userId);
                
                if (success)
                {
                    TempData["Message"] = "You've joined the book club!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to join book club.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { clubId });
        }

        public IActionResult OnPostLeaveClub(int clubId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                bool success = _bookClubBLL.LeaveBookClub(clubId, userId);
                
                if (success)
                {
                    TempData["Message"] = "You've left the book club.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to leave book club.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { clubId });
        }

        public IActionResult OnPostCreateDiscussion(int clubId, string title, string content)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                int postId = _forumBLL.CreateDiscussionPost(clubId, userId, title, content);
                bool success = postId > 0;
                
                if (success)
                {
                    TempData["Message"] = "Discussion created!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to create discussion.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { clubId });
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

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
        public int Id { get; set; }
        
        public int ClubId => Id;

        public BookHub.DAL.BookClub? Club { get; set; }
        public List<BookHub.DAL.BookClubMember> Members { get; set; } = new();
        public List<BookHub.DAL.BookClubMember> PendingMembers { get; set; } = new();
        public List<BookHub.DAL.DiscussionPost> Discussions { get; set; } = new();
        public BookDto? CurrentBook { get; set; }
        public List<BookDto> AllBooks { get; set; } = new();
        public int MemberCount { get; set; }
        public bool IsMember { get; set; }
        public bool IsOwner { get; set; }

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
                IsOwner = Club.OwnerId == userId;

                if (IsOwner)
                {
                    PendingMembers = _bookClubBLL.GetPendingMembers(ClubId, userId);
                    AllBooks = _bookBLL.GetAllBooks().Take(50).ToList();
                }

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

        public IActionResult OnPostRemoveMember(int clubId, int memberUserId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var club = _bookClubBLL.GetBookClubById(clubId);
                if (club == null || club.OwnerId != userId)
                {
                    TempData["Message"] = "You don't have permission to remove members.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { id = clubId });
                }

                bool success = _bookClubBLL.LeaveBookClub(clubId, memberUserId);
                
                if (success)
                {
                    TempData["Message"] = "Member removed successfully.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to remove member.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { id = clubId });
        }

        public IActionResult OnPostUpdateClub(int clubId, string name, string description, string? genre, string? meetingSchedule, int maxMembers)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var club = _bookClubBLL.GetBookClubById(clubId);
                if (club == null || club.OwnerId != userId)
                {
                    TempData["Message"] = "You don't have permission to edit this club.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { id = clubId });
                }

                club.Name = name;
                club.Description = description;
                club.Genre = genre;
                club.MeetingSchedule = meetingSchedule;
                club.MaxMembers = maxMembers;

                bool success = _bookClubBLL.UpdateBookClub(club);
                
                if (success)
                {
                    TempData["Message"] = "Book club updated successfully!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to update book club.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { id = clubId });
        }

        public IActionResult OnPostApproveMember(int clubId, int memberUserId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var club = _bookClubBLL.GetBookClubById(clubId);
                if (club == null || club.OwnerId != userId)
                {
                    TempData["Message"] = "You don't have permission to approve members.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { id = clubId });
                }

                bool success = _bookClubBLL.ApproveMember(clubId, memberUserId, userId);
                
                if (success)
                {
                    TempData["Message"] = "Member approved successfully!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to approve member.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { id = clubId });
        }

        public IActionResult OnPostChangeRole(int clubId, int memberUserId, string newRole)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var club = _bookClubBLL.GetBookClubById(clubId);
                if (club == null || club.OwnerId != userId)
                {
                    TempData["Message"] = "You don't have permission to change member roles.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { id = clubId });
                }

                bool success = _bookClubBLL.ChangeMemberRole(clubId, memberUserId, newRole, userId);
                
                if (success)
                {
                    TempData["Message"] = $"Member role changed to {newRole}!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to change member role.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { id = clubId });
        }

        public IActionResult OnPostSetCurrentBook(int clubId, int bookId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var club = _bookClubBLL.GetBookClubById(clubId);
                if (club == null || club.OwnerId != userId)
                {
                    TempData["Message"] = "You don't have permission to set the current book.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { id = clubId });
                }

                club.CurrentBookId = bookId > 0 ? bookId : null;
                club.CurrentBookStartDate = bookId > 0 ? DateTime.Now : null;
                
                bool success = _bookClubBLL.UpdateBookClub(club);
                
                if (success)
                {
                    TempData["Message"] = bookId > 0 ? "Current book set successfully!" : "Current book cleared.";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to update current book.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { id = clubId });
        }

        public IActionResult OnPostDeleteClub(int clubId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var club = _bookClubBLL.GetBookClubById(clubId);
                if (club == null)
                {
                    TempData["Message"] = "Book club not found.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage("/Community/BookClubs");
                }

                if (club.OwnerId != userId)
                {
                    TempData["Message"] = "You don't have permission to delete this club.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { id = clubId });
                }

                bool success = _bookClubBLL.DeleteBookClub(clubId, userId);
                
                if (success)
                {
                    TempData["Message"] = "Book club deleted successfully.";
                    TempData["MessageType"] = "success";
                    return RedirectToPage("/Community/BookClubs");
                }
                else
                {
                    TempData["Message"] = "Failed to delete book club.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { id = clubId });
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

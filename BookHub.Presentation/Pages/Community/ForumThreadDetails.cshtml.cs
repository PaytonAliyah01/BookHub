using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BookHub.BLL;

namespace BookHub.Presentation.Pages.Community
{
    [Authorize]
    public class ForumThreadDetailsModel : PageModel
    {
        private readonly IForumBLL _forumBLL;

        public ForumThreadDetailsModel(IForumBLL forumBLL)
        {
            _forumBLL = forumBLL;
        }

        [BindProperty(SupportsGet = true)]
        public int PostId { get; set; }

        public BookHub.DAL.DiscussionPost? Post { get; set; }
        public List<BookHub.DAL.DiscussionReply> Replies { get; set; } = new();

        public IActionResult OnGet()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                Post = _forumBLL.GetDiscussionPost(PostId);
                if (Post == null)
                {
                    return Page();
                }

                Replies = Post.Replies;

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error loading forum thread: {ex.Message}";
                TempData["MessageType"] = "error";
                return RedirectToPage("/Community/Forum");
            }
        }

        public IActionResult OnPostAddReply(int postId, string content)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return RedirectToPage("/Auth/Login");
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["Message"] = "Reply content cannot be empty.";
                    TempData["MessageType"] = "error";
                    return RedirectToPage(new { postId });
                }

                int replyId = _forumBLL.CreateDiscussionReply(postId, userId, content.Trim());
                bool success = replyId > 0;
                
                if (success)
                {
                    TempData["Message"] = "Reply posted successfully!";
                    TempData["MessageType"] = "success";
                }
                else
                {
                    TempData["Message"] = "Failed to post reply.";
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
                TempData["MessageType"] = "error";
            }

            return RedirectToPage(new { postId });
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookHub.BLL;
using BookHub.DAL;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BookHub.Presentation.Pages
{
    public class ForumModel : PageModel
    {
        private readonly ForumBLL _forumBLL;
        private readonly BookClubBLL _bookClubBLL;
        private readonly UserBLL _userBLL;
        private readonly IWebHostEnvironment _environment;

        public ForumModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _forumBLL = new ForumBLL(connectionString);
            _bookClubBLL = new BookClubBLL(connectionString);
            _userBLL = new UserBLL(connectionString);
            _environment = environment;
        }

        [BindProperty(SupportsGet = true)]
        public int ClubId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PostId { get; set; }

        [BindProperty(SupportsGet = true)]
        public new int Page { get; set; } = 1;

        [BindProperty]
        public NewPostModel NewPost { get; set; } = new NewPostModel();

        [BindProperty]
        public NewReplyModel NewReply { get; set; } = new NewReplyModel();

        public BookClub? BookClub { get; set; }
        public List<DiscussionPost> Posts { get; set; } = new List<DiscussionPost>();
        public DiscussionPost? CurrentPost { get; set; }
        public bool CanUserPost { get; set; }
        public bool HasMorePosts { get; set; }

        public IActionResult OnGet()
        {
            if (ClubId <= 0)
            {
                return NotFound();
            }

            try
            {
                // Load book club info
                BookClub = _bookClubBLL.GetBookClubById(ClubId);
                if (BookClub == null)
                {
                    return NotFound();
                }

                // Check user permissions
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var user = _userBLL.GetUserProfile(userEmail);
                    if (user != null)
                    {
                        CanUserPost = _forumBLL.CanUserPostInClub(ClubId, user.UserId);
                    }
                }

                if (PostId.HasValue)
                {
                    // Load specific post with replies
                    CurrentPost = _forumBLL.GetDiscussionPost(PostId.Value);
                    if (CurrentPost == null || CurrentPost.ClubId != ClubId)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    // Load posts list
                    Posts = _forumBLL.GetDiscussionPosts(ClubId, Page);
                    
                    // Check if there are more posts
                    var nextPagePosts = _forumBLL.GetDiscussionPosts(ClubId, Page + 1, 1);
                    HasMorePosts = nextPagePosts.Any();
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading forum: {ex.Message}";
                return RedirectToPage("/BookClubs");
            }
        }

        public async Task<IActionResult> OnPostCreatePostAsync()
        {
            try
            {
                // Get current user
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return new JsonResult(new { success = false, message = "You must be logged in to create posts." });
                }

                var user = _userBLL.GetUserProfile(userEmail);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "User not found." });
                }

                // Validate user can post
                if (!_forumBLL.CanUserPostInClub(ClubId, user.UserId))
                {
                    return new JsonResult(new { success = false, message = "You don't have permission to post in this club." });
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(NewPost.Title))
                {
                    return new JsonResult(new { success = false, message = "Title is required." });
                }

                if (string.IsNullOrWhiteSpace(NewPost.Content))
                {
                    return new JsonResult(new { success = false, message = "Content is required." });
                }

                // Process file uploads
                var attachments = await ProcessFileUploadsAsync();

                // Create post
                var postId = _forumBLL.CreateDiscussionPost(
                    ClubId, user.UserId, NewPost.Title, NewPost.Content, false, attachments);

                if (postId > 0)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = "Discussion post created successfully!",
                        postId = postId
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to create discussion post." });
                }
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An error occurred while creating the post." });
            }
        }

        public async Task<IActionResult> OnPostCreateReplyAsync()
        {
            try
            {
                // Get current user
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userEmail))
                {
                    return new JsonResult(new { success = false, message = "You must be logged in to post replies." });
                }

                var user = _userBLL.GetUserProfile(userEmail);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "User not found." });
                }

                if (!PostId.HasValue)
                {
                    return new JsonResult(new { success = false, message = "Invalid post ID." });
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(NewReply.Content))
                {
                    return new JsonResult(new { success = false, message = "Reply content is required." });
                }

                // Process file uploads
                var attachments = await ProcessFileUploadsAsync();

                // Create reply
                var replyId = _forumBLL.CreateDiscussionReply(
                    PostId.Value, user.UserId, NewReply.Content, attachments);

                if (replyId > 0)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = "Reply posted successfully!",
                        replyId = replyId
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Failed to post reply." });
                }
            }
            catch (Exception)
            {
                return new JsonResult(new { success = false, message = "An error occurred while posting the reply." });
            }
        }

        private async Task<List<PostAttachment>> ProcessFileUploadsAsync()
        {
            var attachments = new List<PostAttachment>();
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "forum");
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsPath);

            foreach (var file in Request.Form.Files)
            {
                if (file.Length > 0)
                {
                    // Generate unique filename
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    var relativePath = $"/uploads/forum/{fileName}";

                    // Save file to disk
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Create attachment record
                    var attachment = new PostAttachment
                    {
                        FileName = file.FileName,
                        FileType = file.ContentType,
                        FilePath = relativePath,
                        FileSize = file.Length,
                        UploadedDate = DateTime.Now
                    };

                    attachments.Add(attachment);
                }
            }

            return attachments;
        }
    }

    public class NewPostModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;
    }

    public class NewReplyModel
    {
        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;
    }
}
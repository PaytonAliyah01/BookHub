using BookHub.DAL;

namespace BookHub.BLL
{
    public class ForumBLL
    {
        private readonly BookClubDAL_Simple _dal;

        public ForumBLL(string connectionString)
        {
            _dal = new BookClubDAL_Simple(connectionString);
        }

        // Get discussion posts for a book club
        public List<DiscussionPost> GetDiscussionPosts(int clubId, int page = 1, int pageSize = 20)
        {
            try
            {
                if (clubId <= 0)
                    throw new ArgumentException("Invalid club ID.");

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                int skip = (page - 1) * pageSize;
                return _dal.GetDiscussionPosts(clubId, skip, pageSize);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting discussion posts: {ex.Message}", ex);
            }
        }

        // Get a specific discussion post with replies
        public DiscussionPost? GetDiscussionPost(int postId)
        {
            try
            {
                if (postId <= 0)
                    throw new ArgumentException("Invalid post ID.");

                return _dal.GetDiscussionPost(postId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting discussion post: {ex.Message}", ex);
            }
        }

        // Create a new discussion post
        public int CreateDiscussionPost(int clubId, int userId, string title, string content, bool isSticky = false, List<PostAttachment>? attachments = null)
        {
            try
            {
                // Validation
                if (clubId <= 0)
                    throw new ArgumentException("Invalid club ID.");
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");
                if (string.IsNullOrWhiteSpace(title))
                    throw new ArgumentException("Title is required.");
                if (title.Length > 200)
                    throw new ArgumentException("Title cannot exceed 200 characters.");
                if (string.IsNullOrWhiteSpace(content))
                    throw new ArgumentException("Content is required.");

                // Check if user can post in this club
                if (!_dal.CanUserPostInClub(clubId, userId))
                    throw new UnauthorizedAccessException("You do not have permission to post in this club.");

                // Validate attachments
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        ValidateAttachment(attachment);
                    }
                }

                var post = new DiscussionPost
                {
                    ClubId = clubId,
                    UserId = userId,
                    Title = title.Trim(),
                    Content = ProcessContent(content.Trim()),
                    IsSticky = isSticky,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Attachments = attachments ?? new List<PostAttachment>()
                };

                return _dal.CreateDiscussionPost(post);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating discussion post: {ex.Message}", ex);
            }
        }

        // Create a new reply
        public int CreateDiscussionReply(int postId, int userId, string content, List<PostAttachment>? attachments = null)
        {
            try
            {
                // Validation
                if (postId <= 0)
                    throw new ArgumentException("Invalid post ID.");
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");
                if (string.IsNullOrWhiteSpace(content))
                    throw new ArgumentException("Content is required.");

                // Get the original post to check club permissions
                var originalPost = _dal.GetDiscussionPost(postId);
                if (originalPost == null)
                    throw new InvalidOperationException("Post not found.");

                // Check if user can post in this club
                if (!_dal.CanUserPostInClub(originalPost.ClubId, userId))
                    throw new UnauthorizedAccessException("You do not have permission to reply in this club.");

                // Validate attachments
                if (attachments != null)
                {
                    foreach (var attachment in attachments)
                    {
                        ValidateAttachment(attachment);
                    }
                }

                var reply = new DiscussionReply
                {
                    PostId = postId,
                    UserId = userId,
                    Content = ProcessContent(content.Trim()),
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    Attachments = attachments ?? new List<PostAttachment>()
                };

                return _dal.CreateDiscussionReply(reply);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating discussion reply: {ex.Message}", ex);
            }
        }

        // Check if user can post in club
        public bool CanUserPostInClub(int clubId, int userId)
        {
            try
            {
                if (clubId <= 0 || userId <= 0)
                    return false;

                return _dal.CanUserPostInClub(clubId, userId);
            }
            catch
            {
                return false;
            }
        }

        // Get attachments for a post or reply
        public List<PostAttachment> GetAttachments(int? postId = null, int? replyId = null)
        {
            try
            {
                if (!postId.HasValue && !replyId.HasValue)
                    throw new ArgumentException("Either postId or replyId must be provided.");

                return _dal.GetAttachments(postId, replyId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting attachments: {ex.Message}", ex);
            }
        }

        // Validate file attachment
        private void ValidateAttachment(PostAttachment attachment)
        {
            const long MaxFileSize = 50 * 1024 * 1024; // 50MB
            var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };

            if (string.IsNullOrWhiteSpace(attachment.FileName))
                throw new ArgumentException("File name is required.");

            if (string.IsNullOrWhiteSpace(attachment.FileType))
                throw new ArgumentException("File type is required.");

            if (attachment.FileSize <= 0)
                throw new ArgumentException("File size must be greater than 0.");

            if (attachment.FileSize > MaxFileSize)
                throw new ArgumentException($"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB.");

            // Only allow image files for now
            if (!allowedImageTypes.Contains(attachment.FileType.ToLowerInvariant()))
                throw new ArgumentException("Only image files (JPEG, PNG, GIF, WebP) are allowed.");

            // Validate file extension matches content type
            var extension = Path.GetExtension(attachment.FileName).ToLowerInvariant();
            var expectedExtensions = attachment.FileType.ToLowerInvariant() switch
            {
                "image/jpeg" => new[] { ".jpg", ".jpeg" },
                "image/jpg" => new[] { ".jpg", ".jpeg" },
                "image/png" => new[] { ".png" },
                "image/gif" => new[] { ".gif" },
                "image/webp" => new[] { ".webp" },
                _ => new string[0]
            };

            if (!expectedExtensions.Contains(extension))
                throw new ArgumentException($"File extension {extension} does not match the content type {attachment.FileType}.");
        }

        // Process content for emoji and special formatting
        public string ProcessContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            // Basic content processing
            content = content.Trim();

            // Convert line breaks to HTML
            content = content.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br>");

            // Convert emoji shortcodes to Unicode emojis
            content = ProcessEmojiShortcodes(content);

            return content;
        }

        // Convert emoji shortcodes to Unicode
        private string ProcessEmojiShortcodes(string content)
        {
            var emojiMap = new Dictionary<string, string>
            {
                { ":smile:", "😊" },
                { ":heart:", "❤️" },
                { ":thumbsup:", "👍" },
                { ":thumbsdown:", "👎" },
                { ":laugh:", "😂" },
                { ":cry:", "😢" },
                { ":wink:", "😉" },
                { ":love:", "😍" },
                { ":fire:", "🔥" },
                { ":star:", "⭐" },
                { ":book:", "📚" },
                { ":reading:", "📖" },
                { ":thinking:", "🤔" },
                { ":clap:", "👏" },
                { ":party:", "🎉" },
                { ":cool:", "😎" },
                { ":shocked:", "😱" },
                { ":sleep:", "😴" },
                { ":coffee:", "☕" },
                { ":tea:", "🍵" }
            };

            foreach (var emoji in emojiMap)
            {
                content = content.Replace(emoji.Key, emoji.Value);
            }

            return content;
        }
    }
}
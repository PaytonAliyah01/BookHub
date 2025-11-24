using System.ComponentModel.DataAnnotations;

namespace BookHub.DAL
{
    public class DiscussionPost
    {
        public int PostId { get; set; }
        public int ClubId { get; set; }
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public bool IsSticky { get; set; } = false;
        public int ReplyCount { get; set; } = 0;
        
        // Navigation properties
        public BookClub? BookClub { get; set; }
        public User? User { get; set; }
        public List<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();
        public List<PostAttachment> Attachments { get; set; } = new List<PostAttachment>();
    }
}
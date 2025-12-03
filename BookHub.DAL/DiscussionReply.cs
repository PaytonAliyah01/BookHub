using System.ComponentModel.DataAnnotations;
namespace BookHub.DAL
{
    public class DiscussionReply
    {
        public int ReplyId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public DiscussionPost? DiscussionPost { get; set; }
        public User? User { get; set; }
        public List<PostAttachment> Attachments { get; set; } = new List<PostAttachment>();
    }
}
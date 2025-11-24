using System.ComponentModel.DataAnnotations;

namespace BookHub.DAL
{
    public class BookClubDiscussion
    {
        public int ClubDiscussionId { get; set; }
        public int ClubId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime PostedAt { get; set; }
        
        // Additional properties for display (not in database)
        public int? ReplyToId { get; set; }
        
        // Navigation properties
        public BookClub? BookClub { get; set; }
        public User? User { get; set; }
        public BookClubDiscussion? ReplyTo { get; set; }
    }
}
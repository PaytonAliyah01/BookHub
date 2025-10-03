using System;

namespace BookHub.Models
{
    public class ClubDiscussion
    {
        public int ClubDiscussionId { get; set; }
        public int ClubId { get; set; }
        public BookClub Club { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Message { get; set; } = null!;
        public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    }
}

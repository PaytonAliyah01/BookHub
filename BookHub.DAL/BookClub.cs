using System.ComponentModel.DataAnnotations;
namespace BookHub.DAL
{
    public class BookClub
    {
        public int ClubId { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CurrentBookId { get; set; }
        public DateTime? CurrentBookStartDate { get; set; }
        public DateTime? CurrentBookEndDate { get; set; }
        public bool IsPrivate { get; set; } = false;
        public int MaxMembers { get; set; } = 50;
        [StringLength(200)]
        public string? MeetingSchedule { get; set; }
        [StringLength(100)]
        public string? Genre { get; set; }
        public bool IsActive { get; set; } = true;
        public User? Owner { get; set; }
        public Book? CurrentBook { get; set; }
        public List<BookClubMember> Members { get; set; } = new List<BookClubMember>();
        public int MemberCount { get; set; }
    }
}
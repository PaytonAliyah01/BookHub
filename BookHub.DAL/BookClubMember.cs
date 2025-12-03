using System.ComponentModel.DataAnnotations;
namespace BookHub.DAL
{
    public class BookClubMember
    {
        public int ClubMembershipId { get; set; }
        public int ClubId { get; set; }
        public int UserId { get; set; }
        public bool IsApproved { get; set; } = false;
        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public string Role { get; set; } = "Member";
        public BookClub? BookClub { get; set; }
        public User? User { get; set; }
    }
}
namespace BookHub.Models
{
    public class ClubMembership
    {
        public int ClubMembershipId { get; set; }
        public int ClubId { get; set; }
        public BookClub Club { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}

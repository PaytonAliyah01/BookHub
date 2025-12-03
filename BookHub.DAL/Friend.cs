using System.ComponentModel.DataAnnotations;
namespace BookHub.DAL
{
    public class Friend
    {
        public int FriendshipId { get; set; }
        public int UserId { get; set; }
        public int FriendUserId { get; set; }
        public DateTime FriendsSince { get; set; }
        public User? User { get; set; }
        public User? FriendUser { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
namespace BookHub.DAL
{
    public class FriendRequest
    {
        public int RequestId { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ResponseDate { get; set; }
        public User? FromUser { get; set; }
        public User? ToUser { get; set; }
    }
}
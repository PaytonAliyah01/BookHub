using System.Collections.Generic;

namespace BookHub.Models
{
    public class User
    {
        // Primary Key
        public int UserId { get; set; }

        // Scalar properties
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Salt { get; set; }

        // Profile image with default value
        public string ProfileImage { get; set; } = "default.png";

        // Navigation properties – initialized in constructor
        public ICollection<UserBook> UserBooks { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<ReadingGoal> ReadingGoals { get; set; }
        public ICollection<BookClub> OwnedClubs { get; set; }
        public ICollection<ClubMembership> ClubMemberships { get; set; }
        public ICollection<Friendship> Friendships { get; set; }

        // Constructor
        public User()
        {
            UserBooks = new List<UserBook>();
            Reviews = new List<Review>();
            ReadingGoals = new List<ReadingGoal>();
            OwnedClubs = new List<BookClub>();
            ClubMemberships = new List<ClubMembership>();
            Friendships = new List<Friendship>();
        }
    }
}

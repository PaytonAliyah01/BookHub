using System.Collections.Generic;

namespace BookHub.Models
{
    public class BookClub
    {
        // Primary Key
        public int ClubId { get; set; }

        // Scalar properties
        public required string Name { get; set; }
        public required string Description { get; set; }

        // Owner relationship
        public int OwnerId { get; set; }
        public User? Owner { get; set; }

        // Navigation properties – collections initialized
        public ICollection<ClubMembership> Members { get; set; }
        public ICollection<ClubDiscussion> Discussions { get; set; }

        // Constructor
        public BookClub()
        {
            Members = new List<ClubMembership>();
            Discussions = new List<ClubDiscussion>();
        }
    }
}

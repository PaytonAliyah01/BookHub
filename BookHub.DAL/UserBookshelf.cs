namespace BookHub.DAL
{
    public class UserBookshelf
    {
        public int UserBookId { get; set; }  // Maps to UserBookId in database
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Want to Read"; // Want to Read, Reading, Read
        public bool IsOwned { get; set; } = true; // Maps to IsOwned in database
        public string OwnershipType { get; set; } = "Physical"; // Physical, Digital, or Wishlist
        public int? Rating { get; set; } // 1-5 stars, nullable
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties for joined data
        public Book? Book { get; set; }
        public User? User { get; set; }

        // Convenience property for backward compatibility
        public int UserBookshelfId => UserBookId;
    }
}
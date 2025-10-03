using System.Collections.Generic;

namespace BookHub.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? ISBN { get; set; }
        public string? CoverUrl { get; set; }
        public string? Genre { get; set; }

        public ICollection<UserBook> UserBooks { get; set; }
        public ICollection<Review> Reviews { get; set; }

        public Book()
        {
            UserBooks = new List<UserBook>();
            Reviews = new List<Review>();
        }
    }
}

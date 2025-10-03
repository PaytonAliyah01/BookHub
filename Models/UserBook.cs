using System;

namespace BookHub.Models
{
    public class UserBook
    {
        public int UserBookId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }

        public string? Status { get; set; } // To Read, Reading, Read, Owned
        public bool IsOwned { get; set; }

        public User? User { get; set; }
        public Book? Book { get; set; }
    }
}

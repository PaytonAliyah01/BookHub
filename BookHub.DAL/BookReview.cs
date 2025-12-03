using System;
using System.ComponentModel.DataAnnotations;
namespace BookHub.DAL
{
    public class BookReview
    {
        public int ReviewId { get; set; }
        [Required]
        public int BookId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Review title cannot exceed 100 characters")]
        public string ReviewTitle { get; set; } = string.Empty;
        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 2000 characters")]
        public string ReviewText { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        public bool IsRecommended { get; set; }
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
        public string? BookGenre { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }
    }
}
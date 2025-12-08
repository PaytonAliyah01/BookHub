namespace BookHub.DAL
{
    public class UserBookshelf
    {
        public int UserBookId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Want to Read";
        public bool IsFavorite { get; set; } = false;
        public bool IsOwned { get; set; } = true;
        public string OwnershipType { get; set; } = "Physical";
        public int? Rating { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? DateStarted { get; set; }
        public DateTime? DateFinished { get; set; }
        public int? CurrentPage { get; set; }
        public decimal? ReadingProgress { get; set; }
        public int? TotalPages { get; set; }
        public Book? Book { get; set; }
        public User? User { get; set; }
        public int UserBookshelfId => UserBookId;
        public int DaysReading => DateStarted.HasValue ? (DateFinished ?? DateTime.Now).Subtract(DateStarted.Value).Days : 0;
        public decimal? CalculatedProgress => CurrentPage.HasValue && TotalPages.HasValue && TotalPages > 0 
            ? Math.Round((decimal)CurrentPage.Value / TotalPages.Value * 100, 1) 
            : ReadingProgress;
    }
}
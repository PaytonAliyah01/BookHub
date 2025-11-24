namespace BookHub.DAL
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string CoverUrl { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        
        // Legacy properties for backwards compatibility
        public decimal Price { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Convenience property for template compatibility
        public string ImageUrl => CoverUrl;
    }
}
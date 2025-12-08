namespace BookHub.DAL
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = "default.png";
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public bool IsRestricted { get; set; } = false;
        
        // New profile fields
        public string? Location { get; set; }
        public string? FavoriteGenres { get; set; }
        public string? FavoriteAuthors { get; set; }
        public string? PreferredFormat { get; set; }
        public string? FavoriteQuote { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.Now;
    }
}
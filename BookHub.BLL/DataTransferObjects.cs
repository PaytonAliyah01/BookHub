namespace BookHub.BLL
{
    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string ISBN { get; set; } = "";
        public string CoverUrl { get; set; } = "";
        public string? CoverImagePath { get; set; }
        public string Genre { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime PublicationDate { get; set; }
        public int GenreId { get; set; }
        public int TotalPages { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ReadingGoalDto
    {
        public int ReadingGoalId { get; set; }
        public int UserId { get; set; }
        public int Year { get; set; }
        public int TargetBooks { get; set; }
        public int BooksRead { get; set; }
        public double ProgressPercentage => TargetBooks > 0 ? (double)BooksRead / TargetBooks * 100 : 0;
        public bool IsCompleted => BooksRead >= TargetBooks;
        public int BooksRemaining => Math.Max(0, TargetBooks - BooksRead);
        public string ProgressStatus
        {
            get
            {
                if (IsCompleted) return "Completed";
                var today = DateTime.Now;
                var yearProgress = (today.DayOfYear - 1) / (DateTime.IsLeapYear(Year) ? 366.0 : 365.0) * 100;
                if (ProgressPercentage >= yearProgress)
                    return "On Track";
                else if (ProgressPercentage >= yearProgress * 0.8)
                    return "Behind";
                else
                    return "Far Behind";
            }
        }
    }
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Bio { get; set; } = "";
        public string ProfileImage { get; set; } = "default.png";
    }
    public class BookReviewDto
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public string ReviewTitle { get; set; } = "";
        public string ReviewText { get; set; } = "";
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public DateTime? LastModified { get; set; }
        public bool IsRecommended { get; set; }
        public string Username { get; set; } = "";
        public string BookTitle { get; set; } = "";
        public string BookAuthor { get; set; } = "";
        public string BookGenre { get; set; } = "";
        public string UserEmail { get; set; } = "";
    }
    public class BookClubDto
    {
        public int ClubId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int OwnerId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int MemberCount { get; set; }
        public string OwnerName { get; set; } = "";
        public bool IsCurrentUserMember { get; set; }
        public bool IsCurrentUserOwner { get; set; }
        public string MembershipStatus { get; set; } = ""; 
    }
}
namespace BookHub.Models
{
    public class ReadingGoal
    {
        public int ReadingGoalId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int Year { get; set; }
        public int TargetBooks { get; set; }
        public int BooksRead { get; set; }
    }
}

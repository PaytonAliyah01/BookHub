namespace BookHub.DAL
{
    public class ReadingGoal
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
}
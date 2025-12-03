namespace BookHub.DAL.Interfaces
{
    public interface IAnalyticsDAL
    {
        List<ReadingGoal> GetUserReadingGoals(int userId);
    }
}
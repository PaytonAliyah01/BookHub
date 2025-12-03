namespace BookHub.DAL.Interfaces
{
    public interface IBookClubDAL
    {
        List<ReadingGoal> GetUserReadingGoals(int userId);
        ReadingGoal? GetUserReadingGoalByYear(int userId, int year);
    }
}
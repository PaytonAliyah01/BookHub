using BookHub.DAL;

namespace BookHub.BLL
{
    public class ReadingGoalBLL
    {
        private readonly BookClubDAL_Simple _dal;

        public ReadingGoalBLL(string connectionString)
        {
            _dal = new BookClubDAL_Simple(connectionString);
        }

        public List<BookHub.DAL.ReadingGoal> GetUserReadingGoals(int userId)
        {
            return _dal.GetUserReadingGoals(userId);
        }

        public BookHub.DAL.ReadingGoal? GetCurrentYearGoal(int userId)
        {
            return _dal.GetUserReadingGoalByYear(userId, DateTime.Now.Year);
        }

        public BookHub.DAL.ReadingGoal? GetGoalByYear(int userId, int year)
        {
            return _dal.GetUserReadingGoalByYear(userId, year);
        }

        public string SetReadingGoal(int userId, int year, int targetBooks)
        {
            if (targetBooks < 1)
                return "Target must be at least 1 book";
            
            if (targetBooks > 1000)
                return "Target cannot exceed 1000 books";
            
            if (year < 2020 || year > DateTime.Now.Year + 5)
                return "Year must be between 2020 and " + (DateTime.Now.Year + 5);

            var goal = new BookHub.DAL.ReadingGoal
            {
                UserId = userId,
                Year = year,
                TargetBooks = targetBooks,
                BooksRead = 0
            };

            // If updating current year goal, preserve books read count
            var existingGoal = _dal.GetUserReadingGoalByYear(userId, year);
            if (existingGoal != null)
            {
                goal.BooksRead = existingGoal.BooksRead;
            }

            int result = _dal.CreateOrUpdateReadingGoal(goal);
            return result > 0 ? "Success" : "Failed to save reading goal";
        }

        public string UpdateProgress(int userId, int year, int booksRead)
        {
            if (booksRead < 0)
                return "Books read cannot be negative";

            if (booksRead > 1000)
                return "Books read cannot exceed 1000";

            var goal = _dal.GetUserReadingGoalByYear(userId, year);
            if (goal == null)
                return "No reading goal found for this year";

            bool success = _dal.UpdateBooksRead(userId, year, booksRead);
            return success ? "Success" : "Failed to update progress";
        }

        public string IncrementProgress(int userId, int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            
            var goal = _dal.GetUserReadingGoalByYear(userId, year);
            if (goal == null)
                return "No reading goal found for this year";

            bool success = _dal.UpdateBooksRead(userId, year, goal.BooksRead + 1);
            return success ? "Success" : "Failed to update progress";
        }

        public string DecrementProgress(int userId, int year = 0)
        {
            if (year == 0) year = DateTime.Now.Year;
            
            var goal = _dal.GetUserReadingGoalByYear(userId, year);
            if (goal == null)
                return "No reading goal found for this year";

            if (goal.BooksRead <= 0)
                return "Cannot reduce below 0 books";

            bool success = _dal.UpdateBooksRead(userId, year, goal.BooksRead - 1);
            return success ? "Success" : "Failed to update progress";
        }

        public string DeleteReadingGoal(int readingGoalId)
        {
            bool success = _dal.DeleteReadingGoal(readingGoalId);
            return success ? "Success" : "Failed to delete reading goal";
        }

        public List<int> GetAvailableYears()
        {
            var years = new List<int>();
            int currentYear = DateTime.Now.Year; // This should be 2025
            
            // Add current year and next 5 years (no previous years)
            for (int i = currentYear; i <= currentYear + 5; i++)
            {
                years.Add(i);
            }
            
            // Should return: 2025, 2026, 2027, 2028, 2029, 2030
            return years;
        }

        public string GetMotivationalMessage(BookHub.DAL.ReadingGoal? goal)
        {
            if (goal == null) return "Set a reading goal to get started!";

            if (goal.IsCompleted)
                return $"🎉 Congratulations! You've achieved your goal of {goal.TargetBooks} books!";

            var today = DateTime.Now;
            var daysInYear = DateTime.IsLeapYear(goal.Year) ? 366 : 365;
            var daysPassed = today.DayOfYear - 1;
            var daysRemaining = daysInYear - daysPassed;

            var expectedProgress = (double)daysPassed / daysInYear * goal.TargetBooks;
            
            if (goal.BooksRead >= expectedProgress)
            {
                return $"📚 Great job! You're on track to reach your goal. {goal.BooksRemaining} books to go!";
            }
            else
            {
                var booksNeededPerWeek = Math.Ceiling((double)goal.BooksRemaining / (daysRemaining / 7.0));
                return $"📖 You need to read about {booksNeededPerWeek} book(s) per week to reach your goal. You can do it!";
            }
        }

        public Dictionary<string, object> GetProgressAnalytics(BookHub.DAL.ReadingGoal? goal)
        {
            var analytics = new Dictionary<string, object>();
            
            if (goal == null)
            {
                analytics["HasGoal"] = false;
                return analytics;
            }

            var today = DateTime.Now;
            var startOfYear = new DateTime(goal.Year, 1, 1);
            var endOfYear = new DateTime(goal.Year, 12, 31);
            var daysInYear = DateTime.IsLeapYear(goal.Year) ? 366 : 365;
            
            // Only calculate for current or past years
            var daysPassed = goal.Year == today.Year ? today.DayOfYear - 1 : 
                           goal.Year < today.Year ? daysInYear : 0;
            var daysRemaining = goal.Year == today.Year ? daysInYear - daysPassed :
                              goal.Year > today.Year ? daysInYear : 0;
            
            var expectedProgress = daysPassed > 0 ? (double)daysPassed / daysInYear * goal.TargetBooks : 0;
            var actualProgress = goal.BooksRead;
            var progressDifference = actualProgress - expectedProgress;
            
            analytics["HasGoal"] = true;
            analytics["DaysPassed"] = daysPassed;
            analytics["DaysRemaining"] = daysRemaining;
            analytics["ExpectedBooks"] = Math.Round(expectedProgress, 1);
            analytics["ActualBooks"] = actualProgress;
            analytics["ProgressDifference"] = Math.Round(progressDifference, 1);
            analytics["IsAhead"] = progressDifference > 0;
            analytics["DailyTarget"] = Math.Round((double)goal.TargetBooks / daysInYear, 2);
            analytics["CurrentPace"] = daysPassed > 0 ? Math.Round((double)actualProgress / daysPassed * daysInYear, 1) : 0;
            analytics["BooksNeededPerWeek"] = daysRemaining > 0 ? Math.Ceiling((double)goal.BooksRemaining / (daysRemaining / 7.0)) : 0;
            analytics["BooksNeededPerMonth"] = daysRemaining > 0 ? Math.Ceiling((double)goal.BooksRemaining / (daysRemaining / 30.0)) : 0;
            analytics["YearProgress"] = Math.Round((double)daysPassed / daysInYear * 100, 1);
            analytics["IsCurrentYear"] = goal.Year == today.Year;
            analytics["IsFutureYear"] = goal.Year > today.Year;
            
            return analytics;
        }
    }
}
using BookHub.DAL;
using BookHub.DAL.Interfaces;
using System.Globalization;
namespace BookHub.BLL
{
    public class AnalyticsBLL : IAnalyticsBLL
    {
        private readonly IUserBookshelfDAL _userBookshelfDAL;
        private readonly IBookClubDAL _generalDAL;
        private readonly IBookReviewDAL _reviewDAL;
        public AnalyticsBLL(IUserBookshelfDAL userBookshelfDAL, IBookClubDAL generalDAL, IBookReviewDAL reviewDAL)
        {
            _userBookshelfDAL = userBookshelfDAL;
            _generalDAL = generalDAL;
            _reviewDAL = reviewDAL;
        }
        public Dictionary<string, object> GetReadingProgressData(int userId)
        {
            var data = new Dictionary<string, object>();
            try
            {
                var readingGoals = _generalDAL.GetUserReadingGoals(userId);
                var monthlyProgress = GetMonthlyReadingProgress(userId);
                var genreBreakdown = GetGenreAnalytics(userId);
                var readingStats = GetReadingStatistics(userId);
                data["ReadingGoalsHistory"] = readingGoals.Select(g => new {
                    year = g.Year,
                    target = g.TargetBooks,
                    actual = g.BooksRead,
                    percentage = g.ProgressPercentage
                }).ToList();
                data["MonthlyProgress"] = monthlyProgress;
                data["GenreBreakdown"] = genreBreakdown;
                data["ReadingStats"] = readingStats;
                var currentYearGoal = readingGoals?.FirstOrDefault(g => g.Year == DateTime.Now.Year);
                if (currentYearGoal != null)
                {
                    data["CurrentYearGoal"] = currentYearGoal;
                }
                else
                {
                    data["CurrentYearGoal"] = new { year = DateTime.Now.Year, target = 0, actual = 0, percentage = 0.0 };
                }
            }
            catch (Exception ex)
            {
                data["Error"] = $"Error loading reading progress: {ex.Message}";
            }
            return data;
        }
        public Dictionary<string, object> GetReviewAnalytics(int userId)
        {
            var data = new Dictionary<string, object>();
            try
            {
                data["RatingDistribution"] = new Dictionary<string, int>();
                data["MonthlyReviews"] = new List<object>();
                data["AverageRatingOverTime"] = new List<object>();
                data["TotalReviews"] = 0;
                data["AverageRating"] = 0.0;
            }
            catch (Exception ex)
            {
                data["Error"] = $"Error loading review analytics: {ex.Message}";
            }
            return data;
        }
        public Dictionary<string, object> GetBookshelfAnalytics(int userId)
        {
            var data = new Dictionary<string, object>();
            try
            {
                var userBooks = _userBookshelfDAL.GetUserBookshelf(userId);
                var statusBreakdown = userBooks
                    .GroupBy(b => b.Status)
                    .ToDictionary(g => g.Key, g => g.Count());
                var ownershipBreakdown = userBooks
                    .GroupBy(b => b.OwnershipType)
                    .ToDictionary(g => g.Key, g => g.Count());
                var genreStats = GetGenreStatsFromBooks(userBooks);
                var booksAddedOverTime = userBooks
                    .GroupBy(b => new { b.DateAdded.Year, b.DateAdded.Month })
                    .Select(g => new {
                        month = new DateTime(g.Key.Year, g.Key.Month, 1, 0, 0, 0, DateTimeKind.Unspecified).ToString("MMM yyyy"),
                        count = g.Count()
                    })
                    .OrderBy(x => x.month)
                    .ToList();
                data["StatusBreakdown"] = statusBreakdown;
                data["OwnershipBreakdown"] = ownershipBreakdown;
                data["GenreStats"] = genreStats;
                data["BooksAddedOverTime"] = booksAddedOverTime;
                data["TotalBooks"] = userBooks.Count;
            }
            catch (Exception ex)
            {
                data["Error"] = $"Error loading bookshelf analytics: {ex.Message}";
            }
            return data;
        }
        private List<object> GetMonthlyReadingProgress(int userId)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var userBooks = _userBookshelfDAL.GetUserBookshelf(userId)
                    .Where(b => b.Status == "Read" && b.DateAdded.Year == currentYear)
                    .ToList();
                var monthlyData = new List<object>();
                for (int month = 1; month <= 12; month++)
                {
                    var booksThisMonth = userBooks.Count(b => b.DateAdded.Month == month);
                    monthlyData.Add(new {
                        month = new DateTime(currentYear, month, 1).ToString("MMM"),
                        books = booksThisMonth
                    });
                }
                return monthlyData;
            }
            catch
            {
                return new List<object>();
            }
        }
        private Dictionary<string, int> GetGenreAnalytics(int userId)
        {
            try
            {
                var statusStats = _userBookshelfDAL.GetUserBookshelfStats(userId);
                return statusStats;
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }
        private Dictionary<string, int> GetGenreStatsFromBooks(List<UserBookshelf> userBooks)
        {
            var genreStats = new Dictionary<string, int>
            {
                {"Fiction", 0},
                {"Non-Fiction", 0},
                {"Science Fiction", 0},
                {"Mystery", 0},
                {"Romance", 0},
                {"Other", 0}
            };
            foreach (var book in userBooks)
            {
                if (book.Book != null && !string.IsNullOrEmpty(book.Book.Title))
                {
                    var title = book.Book.Title.ToLower();
                    if (title.Contains("science") || title.Contains("sci-fi"))
                        genreStats["Science Fiction"]++;
                    else if (title.Contains("mystery") || title.Contains("crime"))
                        genreStats["Mystery"]++;
                    else if (title.Contains("romance") || title.Contains("love"))
                        genreStats["Romance"]++;
                    else if (title.Contains("history") || title.Contains("biography"))
                        genreStats["Non-Fiction"]++;
                    else
                        genreStats["Fiction"]++;
                }
                else
                {
                    genreStats["Other"]++;
                }
            }
            return genreStats.Where(kvp => kvp.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        private Dictionary<string, object> GetReadingStatistics(int userId)
        {
            var stats = new Dictionary<string, object>();
            try
            {
                var userBooks = _userBookshelfDAL.GetUserBookshelf(userId);
                var readBooks = userBooks.Where(b => b.Status == "Read").ToList();
                stats["TotalBooksRead"] = readBooks.Count;
                stats["BooksCurrentlyReading"] = userBooks.Count(b => b.Status == "Reading");
                stats["BooksWantToRead"] = userBooks.Count(b => b.Status == "Want to Read");
                stats["TotalBooksInLibrary"] = userBooks.Count;
                var currentStreak = CalculateReadingStreak(readBooks);
                stats["CurrentReadingStreak"] = currentStreak;
                var statusStats = _userBookshelfDAL.GetUserBookshelfStats(userId);
                var topStatus = statusStats.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
                stats["FavoriteGenre"] = topStatus.Key ?? "None";
                stats["FavoriteGenreCount"] = topStatus.Value;
                var currentGoal = _generalDAL.GetUserReadingGoalByYear(userId, DateTime.Now.Year);
                if (currentGoal != null)
                {
                    stats["YearlyGoalProgress"] = currentGoal.ProgressPercentage;
                    stats["YearlyGoalTarget"] = currentGoal.TargetBooks;
                    stats["YearlyGoalRead"] = currentGoal.BooksRead;
                }
            }
            catch
            {
                stats["TotalBooksRead"] = 0;
                stats["BooksCurrentlyReading"] = 0;
                stats["BooksWantToRead"] = 0;
                stats["TotalBooksInLibrary"] = 0;
                stats["CurrentReadingStreak"] = 0;
                stats["FavoriteGenre"] = "None";
                stats["FavoriteGenreCount"] = 0;
            }
            return stats;
        }
        private int CalculateReadingStreak(List<UserBookshelf> readBooks)
        {
            if (!readBooks.Any()) return 0;
            var sortedBooks = readBooks
                .OrderByDescending(b => b.DateAdded)
                .ToList();
            if (!sortedBooks.Any()) return 0;
            var streak = 1;
            var lastDate = sortedBooks.First().DateAdded.Date;
            for (int i = 1; i < sortedBooks.Count; i++)
            {
                var currentDate = sortedBooks[i].DateAdded.Date;
                var daysDifference = (lastDate - currentDate).Days;
                if (daysDifference <= 7)
                {
                    streak++;
                    lastDate = currentDate;
                }
                else
                {
                    break;
                }
            }
            return streak;
        }
    }
}
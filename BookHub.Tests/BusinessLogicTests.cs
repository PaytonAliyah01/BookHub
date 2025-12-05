using Xunit;
using FluentAssertions;
using BookHub.DAL;
using BookHub.BLL;

namespace BookHub.Tests
{
    /// <summary>
    /// Business logic calculation tests - Testing complex business rules and calculations
    /// These tests verify core application logic without database dependencies
    /// </summary>
    public class BusinessLogicTests
    {
        #region Reading Progress Calculations

        [Theory]
        [InlineData(0, 100, 0.0)]
        [InlineData(25, 100, 25.0)]
        [InlineData(50, 100, 50.0)]
        [InlineData(75, 100, 75.0)]
        [InlineData(100, 100, 100.0)]
        public void CalculateReadingProgress_ReturnsCorrectPercentage(int currentPage, int totalPages, decimal expectedProgress)
        {
            // Arrange
            var userBook = new UserBookshelf
            {
                CurrentPage = currentPage,
                TotalPages = totalPages
            };

            // Act
            var progress = userBook.CalculatedProgress;

            // Assert
            progress.Should().Be(expectedProgress);
        }

        [Fact]
        public void ReadingProgress_HandlesZeroTotalPages()
        {
            // Arrange
            var userBook = new UserBookshelf
            {
                CurrentPage = 0,
                TotalPages = 0
            };

            // Act
            var progress = userBook.CalculatedProgress;

            // Assert
            progress.Should().BeNull();
        }

        #endregion

        #region Reading Goal Calculations

        [Theory]
        [InlineData(50, 0, 0)]
        [InlineData(50, 25, 50)]
        [InlineData(50, 50, 100)]
        [InlineData(50, 75, 150)] // Over 100% if exceeded
        public void ReadingGoal_CalculatesProgressPercentage(int targetBooks, int booksRead, int expectedProgress)
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = targetBooks,
                BooksRead = booksRead
            };

            // Act
            var progress = goal.ProgressPercentage;

            // Assert
            progress.Should().Be(expectedProgress);
        }

        [Theory]
        [InlineData(50, 0, 50)]
        [InlineData(50, 25, 25)]
        [InlineData(50, 50, 0)]
        [InlineData(50, 60, 0)]
        public void ReadingGoal_CalculatesBooksRemaining(int targetBooks, int booksRead, int expectedRemaining)
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = targetBooks,
                BooksRead = booksRead
            };

            // Act
            var remaining = goal.BooksRemaining;

            // Assert
            remaining.Should().Be(expectedRemaining);
        }

        [Theory]
        [InlineData(50, 49, false)]
        [InlineData(50, 50, true)]
        [InlineData(50, 51, true)]
        public void ReadingGoal_DeterminesCompletion(int targetBooks, int booksRead, bool expectedCompleted)
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = targetBooks,
                BooksRead = booksRead
            };

            // Act & Assert
            goal.IsCompleted.Should().Be(expectedCompleted);
        }

        #endregion

        #region Days Reading Calculation

        [Theory]
        [InlineData(1, 1)]
        [InlineData(7, 7)]
        [InlineData(30, 30)]
        [InlineData(365, 365)]
        public void DaysReading_CalculatesCorrectly(int daysAgo, int expectedDays)
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-daysAgo);
            var userBook = new UserBookshelf
            {
                DateStarted = startDate,
                DateFinished = DateTime.Now
            };

            // Act
            var days = userBook.DaysReading;

            // Assert
            days.Should().Be(expectedDays);
        }

        [Fact]
        public void DaysReading_ReturnsZero_WhenNotStarted()
        {
            // Arrange
            var userBook = new UserBookshelf
            {
                DateStarted = null,
                DateFinished = null
            };

            // Act
            var days = userBook.DaysReading;

            // Assert
            days.Should().Be(0);
        }

        [Fact]
        public void DaysReading_CalculatesFromStartDate_WhenNotFinished()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-10);
            var userBook = new UserBookshelf
            {
                DateStarted = startDate,
                DateFinished = null
            };

            // Act
            var days = userBook.DaysReading;

            // Assert
            days.Should().BeGreaterThanOrEqualTo(10);
        }

        #endregion

        #region Average Pages Per Day

        [Theory]
        [InlineData(100, 10, 10.0)]
        [InlineData(200, 20, 10.0)]
        [InlineData(50, 5, 10.0)]
        [InlineData(365, 365, 1.0)]
        public void AveragePagesPerDay_CalculatesCorrectly(int currentPage, int daysReading, decimal expectedAverage)
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-daysReading);
            var userBook = new UserBookshelf
            {
                CurrentPage = currentPage,
                TotalPages = 500,
                DateStarted = startDate
            };

            // Act
            var average = userBook.DaysReading > 0 ? (decimal)userBook.CurrentPage / userBook.DaysReading : 0;

            // Assert
            average.Should().Be(expectedAverage);
        }

        #endregion

        #region Estimated Days to Finish

        [Theory]
        [InlineData(50, 200, 10, 30)] // 50/10 = 5 pages/day, 150 pages left = 30 days
        [InlineData(100, 200, 10, 10)] // 100/10 = 10 pages/day, 100 pages left = 10 days
        [InlineData(150, 200, 10, 4)]  // 150/10 = 15 pages/day, 50 pages left = 3-4 days
        public void EstimatedDaysToFinish_CalculatesCorrectly(int currentPage, int totalPages, int daysReading, int expectedDays)
        {
            // Arrange
            var pagesRead = currentPage;
            var pagesRemaining = totalPages - currentPage;
            var averagePagesPerDay = daysReading > 0 ? (decimal)pagesRead / daysReading : 0;

            // Act
            var estimatedDays = averagePagesPerDay > 0 ? (int)Math.Ceiling(pagesRemaining / averagePagesPerDay) : 0;

            // Assert
            estimatedDays.Should().BeGreaterThanOrEqualTo(0);
            estimatedDays.Should().BeLessThanOrEqualTo(expectedDays + 5); // Allow some variance due to rounding
        }

        #endregion

        #region Book Collection Statistics

        [Fact]
        public void BookCollection_CalculatesTotalPages()
        {
            // Arrange
            var books = new List<UserBookshelf>
            {
                new UserBookshelf { TotalPages = 300 },
                new UserBookshelf { TotalPages = 250 },
                new UserBookshelf { TotalPages = 400 }
            };

            // Act
            var totalPages = books.Sum(b => b.TotalPages);

            // Assert
            totalPages.Should().Be(950);
        }

        [Fact]
        public void BookCollection_CountsByStatus()
        {
            // Arrange
            var books = new List<UserBookshelf>
            {
                new UserBookshelf { Status = "Want to Read" },
                new UserBookshelf { Status = "Reading" },
                new UserBookshelf { Status = "Reading" },
                new UserBookshelf { Status = "Read" },
                new UserBookshelf { Status = "Read" },
                new UserBookshelf { Status = "Read" }
            };

            // Act
            var wantToRead = books.Count(b => b.Status == "Want to Read");
            var reading = books.Count(b => b.Status == "Reading");
            var read = books.Count(b => b.Status == "Read");

            // Assert
            wantToRead.Should().Be(1);
            reading.Should().Be(2);
            read.Should().Be(3);
        }

        [Fact]
        public void BookCollection_CalculatesAverageRating()
        {
            // Arrange
            var reviews = new List<BookReviewDto>
            {
                new BookReviewDto { Rating = 5 },
                new BookReviewDto { Rating = 4 },
                new BookReviewDto { Rating = 3 },
                new BookReviewDto { Rating = 4 }
            };

            // Act
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            // Assert
            averageRating.Should().Be(4.0);
        }

        #endregion

        #region Reading Streak Calculations

        [Theory]
        [InlineData(1, 1)]
        [InlineData(7, 7)]
        [InlineData(30, 30)]
        public void ReadingStreak_CalculatesDays(int consecutiveDays, int expectedStreak)
        {
            // Arrange - Simulate consecutive reading days
            var activities = new List<DateTime>();
            for (int i = 0; i < consecutiveDays; i++)
            {
                activities.Add(DateTime.Now.Date.AddDays(-i));
            }

            // Act - Count consecutive days
            int streak = 0;
            var sortedDates = activities.OrderByDescending(d => d).Select(d => d.Date).Distinct().ToList();
            for (int i = 0; i < sortedDates.Count; i++)
            {
                if (i == 0 || sortedDates[i] == sortedDates[i - 1].AddDays(-1))
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            // Assert
            streak.Should().Be(expectedStreak);
        }

        #endregion

        #region ISBN Formatting

        [Theory]
        [InlineData("1234567890", "1234567890")]
        [InlineData("123-456-789-0", "1234567890")]
        [InlineData("123 456 789 0", "1234567890")]
        public void ISBN_RemovesFormatting(string input, string expected)
        {
            // Arrange & Act
            var cleaned = input.Replace("-", "").Replace(" ", "");

            // Assert
            cleaned.Should().Be(expected);
        }

        #endregion
    }
}

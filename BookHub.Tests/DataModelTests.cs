using Xunit;
using FluentAssertions;
using BookHub.DAL;
using BookHub.BLL;

namespace BookHub.Tests
{
    /// <summary>
    /// Unit tests for Data Models - Testing entity validation and properties
    /// These tests validate business logic WITHOUT touching the database
    /// </summary>
    public class DataModelTests
    {
        [Fact]
        public void ReadingGoalDto_CalculatesProgressPercentage_Correctly()
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = 50,
                BooksRead = 25
            };

            // Act
            var progress = goal.ProgressPercentage;

            // Assert
            progress.Should().Be(50);
        }

        [Fact]
        public void ReadingGoalDto_IsCompleted_WhenTargetReached()
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = 50,
                BooksRead = 50
            };

            // Act & Assert
            goal.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void ReadingGoalDto_IsNotCompleted_WhenTargetNotReached()
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = 50,
                BooksRead = 25
            };

            // Act & Assert
            goal.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public void ReadingGoalDto_CalculatesBooksRemaining_Correctly()
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = 50,
                BooksRead = 30
            };

            // Act
            var remaining = goal.BooksRemaining;

            // Assert
            remaining.Should().Be(20);
        }

        [Fact]
        public void ReadingGoalDto_BooksRemaining_IsZero_WhenCompleted()
        {
            // Arrange
            var goal = new ReadingGoalDto
            {
                TargetBooks = 50,
                BooksRead = 60
            };

            // Act
            var remaining = goal.BooksRemaining;

            // Assert
            remaining.Should().Be(0);
        }

        [Fact]
        public void UserBookshelf_CalculatesReadingProgress_Correctly()
        {
            // Arrange
            var userBook = new UserBookshelf
            {
                CurrentPage = 50,
                TotalPages = 200
            };

            // Act
            var progress = userBook.CalculatedProgress;

            // Assert
            progress.Should().Be(25.0m);
        }

        [Fact]
        public void UserBookshelf_CalculatesDaysReading_Correctly()
        {
            // Arrange
            var startDate = DateTime.Now.AddDays(-10);
            var userBook = new UserBookshelf
            {
                DateStarted = startDate,
                DateFinished = DateTime.Now
            };

            // Act
            var days = userBook.DaysReading;

            // Assert
            days.Should().Be(10);
        }

        [Fact]
        public void BookDto_HasAllRequiredProperties()
        {
            // Arrange & Act
            var book = new BookDto
            {
                BookId = 1,
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                Genre = "Fiction",
                Description = "A test book"
            };

            // Assert
            book.BookId.Should().Be(1);
            book.Title.Should().Be("Test Book");
            book.Author.Should().Be("Test Author");
            book.ISBN.Should().Be("1234567890");
            book.Genre.Should().Be("Fiction");
            book.Description.Should().Be("A test book");
        }

        [Fact]
        public void UserDto_HasAllRequiredProperties()
        {
            // Arrange & Act
            var user = new UserDto
            {
                UserId = 1,
                Name = "John Doe",
                Email = "john@example.com",
                Bio = "Book lover"
            };

            // Assert
            user.UserId.Should().Be(1);
            user.Name.Should().Be("John Doe");
            user.Email.Should().Be("john@example.com");
            user.Bio.Should().Be("Book lover");
        }

        [Fact]
        public void BookReviewDto_CalculatesRecommendation()
        {
            // Arrange
            var review = new BookReviewDto
            {
                ReviewId = 1,
                UserId = 1,
                BookId = 1,
                Rating = 5,
                ReviewText = "Excellent book!"
            };

            // Act & Assert
            review.Rating.Should().Be(5);
            review.ReviewText.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("Want to Read")]
        [InlineData("Reading")]
        [InlineData("Read")]
        public void UserBookshelf_Status_AcceptsValidValues(string status)
        {
            // Arrange & Act
            var userBook = new UserBookshelf
            {
                Status = status
            };

            // Assert
            userBook.Status.Should().Be(status);
        }

        [Theory]
        [InlineData("Physical")]
        [InlineData("eBook")]
        [InlineData("Audiobook")]
        public void UserBookshelf_OwnershipType_AcceptsValidValues(string ownershipType)
        {
            // Arrange & Act
            var userBook = new UserBookshelf
            {
                OwnershipType = ownershipType
            };

            // Assert
            userBook.OwnershipType.Should().Be(ownershipType);
        }

        [Fact]
        public void BookClubDto_HasMemberCountProperty()
        {
            // Arrange & Act
            var club = new BookClubDto
            {
                ClubId = 1,
                Name = "Fiction Lovers",
                Description = "We love fiction",
                MemberCount = 25
            };

            // Assert
            club.MemberCount.Should().Be(25);
            club.Name.Should().Be("Fiction Lovers");
        }
    }
}

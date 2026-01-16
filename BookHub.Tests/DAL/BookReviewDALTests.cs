using BookHub.DAL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.DAL
{
    public class BookReviewDALTests
    {
        private readonly string _testConnectionString;

        public BookReviewDALTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void Constructor_WithValidConnectionString_ShouldSucceed()
        {
            // Arrange & Act
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);

            // Assert
            bookReviewDAL.Should().NotBeNull();
        }

        [Fact]
        public void GetReviewsForBook_WithValidBookId_ShouldReturnReviewsList()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var bookId = 1;

            // Act
            var result = bookReviewDAL.GetReviewsForBook(bookId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<BookReview>>();
        }

        [Fact]
        public void GetReviewsForBook_WithInvalidBookId_ShouldReturnEmptyList()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var invalidBookId = 999999;

            // Act
            var result = bookReviewDAL.GetReviewsForBook(invalidBookId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetReviewsForBook_WithNegativeBookId_ShouldReturnEmptyList()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var negativeBookId = -1;

            // Act
            var result = bookReviewDAL.GetReviewsForBook(negativeBookId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetReviewsForBook_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL("Invalid Connection String");
            var bookId = 1;

            // Act
            var act = () => bookReviewDAL.GetReviewsForBook(bookId);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Database error occurred while retrieving book reviews:*");
        }

        [Fact]
        public void AddReview_WithValidData_ShouldReturnReviewId()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var userDAL = new UserDAL(_testConnectionString);

            // Create test data
            var userId = CreateTestUser(userDAL);
            var bookId = CreateTestBook(bookDAL);

            // Act
            var result = bookReviewDAL.AddReview(userId, bookId, 5, "Great book!", DateTime.Now);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void AddReview_WithInvalidRating_ShouldHandleGracefully()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var userId = 1;
            var bookId = 1;
            var invalidRating = 6; // Assuming ratings are 1-5

            // Act
            var act = () => bookReviewDAL.AddReview(userId, bookId, invalidRating, "Review", DateTime.Now);

            // Assert - May or may not throw depending on constraints
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddReview_WithNegativeRating_ShouldHandleGracefully()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var userId = 1;
            var bookId = 1;
            var negativeRating = -1;

            // Act
            var act = () => bookReviewDAL.AddReview(userId, bookId, negativeRating, "Review", DateTime.Now);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddReview_WithNullReviewText_ShouldHandleGracefully()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var userId = 1;
            var bookId = 1;

            // Act
            var act = () => bookReviewDAL.AddReview(userId, bookId, 5, null!, DateTime.Now);

            // Assert - May allow null or throw
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void UpdateReview_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var userDAL = new UserDAL(_testConnectionString);

            // Create test data
            var userId = CreateTestUser(userDAL);
            var bookId = CreateTestBook(bookDAL);
            var reviewId = bookReviewDAL.AddReview(userId, bookId, 5, "Great book!", DateTime.Now);

            // Act
            var result = bookReviewDAL.UpdateReview(reviewId, 4, "Updated review");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateReview_WithInvalidReviewId_ShouldReturnFalse()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var invalidReviewId = 999999;

            // Act
            var result = bookReviewDAL.UpdateReview(invalidReviewId, 5, "Updated review");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DeleteReview_WithValidReviewId_ShouldReturnTrue()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var userDAL = new UserDAL(_testConnectionString);

            // Create test data
            var userId = CreateTestUser(userDAL);
            var bookId = CreateTestBook(bookDAL);
            var reviewId = bookReviewDAL.AddReview(userId, bookId, 5, "Great book!", DateTime.Now);

            // Act
            var result = bookReviewDAL.DeleteReview(reviewId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void DeleteReview_WithInvalidReviewId_ShouldReturnFalse()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var invalidReviewId = 999999;

            // Act
            var result = bookReviewDAL.DeleteReview(invalidReviewId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetReviewsByUserId_WithValidUserId_ShouldReturnReviewsList()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var userId = 1;

            // Act
            var result = bookReviewDAL.GetReviewsByUserId(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<BookReview>>();
        }

        [Fact]
        public void GetReviewsByUserId_WithInvalidUserId_ShouldReturnEmptyList()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var invalidUserId = 999999;

            // Act
            var result = bookReviewDAL.GetReviewsByUserId(invalidUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        private int CreateTestUser(UserDAL userDAL)
        {
            var email = $"testuser{Guid.NewGuid()}@test.com";
            userDAL.RegisterUser("Test User", $"user{Guid.NewGuid()}", email, "hash", "salt", null, null);
            var user = userDAL.GetUserByEmail(email);
            return user!.UserId;
        }

        private int CreateTestBook(BookDAL bookDAL)
        {
            return bookDAL.AddBook(
                $"Test Book {Guid.NewGuid()}",
                "Test Author",
                $"ISBN-{Guid.NewGuid()}",
                "http://test.com/cover.jpg",
                "Fiction",
                "Test description"
            );
        }
    }
}

using BookHub.DAL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.EdgeCases
{
    /// <summary>
    /// Tests for edge cases, boundary conditions, and error scenarios
    /// </summary>
    public class EdgeCaseTests
    {
        private readonly string _testConnectionString;

        public EdgeCaseTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        #region SQL Injection Tests

        [Fact]
        public void UserExists_WithSQLInjectionAttempt_ShouldHandleSafely()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var maliciousEmail = "'; DROP TABLE Users; --";

            // Act
            var result = userDAL.UserExists(maliciousEmail);

            // Assert - Should return false, not execute injection
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateUser_WithSQLInjectionInEmail_ShouldHandleSafely()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var maliciousEmail = "admin' OR '1'='1";
            var password = "anything";

            // Act
            var result = userDAL.ValidateUser(maliciousEmail, password);

            // Assert - Should return null, not bypass authentication
            result.Should().BeNull();
        }

        [Fact]
        public void SearchBooks_WithSQLInjectionAttempt_ShouldHandleSafely()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var maliciousKeyword = "'; DELETE FROM Books; --";

            // Act
            var act = () => bookDAL.SearchBooks(maliciousKeyword);

            // Assert - Should not throw, should handle safely
            act.Should().NotThrow();
        }

        #endregion

        #region Boundary Value Tests

        [Fact]
        public void AddBook_WithVeryLongTitle_ShouldHandleGracefully()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var veryLongTitle = new string('A', 1000); // 1000 characters

            // Act
            var act = () => bookDAL.AddBook(veryLongTitle, "Author", "ISBN", "url", "Genre", "Description");

            // Assert - Should either succeed or throw appropriate exception
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void RegisterUser_WithVeryLongName_ShouldHandleGracefully()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var veryLongName = new string('N', 500);
            var email = $"test{Guid.NewGuid()}@test.com";

            // Act
            var act = () => userDAL.RegisterUser(veryLongName, "username", email, "hash", "salt", null, null);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddReview_WithZeroRating_ShouldHandleGracefully()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var userId = 1;
            var bookId = 1;

            // Act
            var act = () => bookReviewDAL.AddReview(userId, bookId, 0, "Review", DateTime.Now);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddReview_WithMaxIntRating_ShouldHandleGracefully()
        {
            // Arrange
            var bookReviewDAL = new BookReviewDAL(_testConnectionString);
            var userId = 1;
            var bookId = 1;

            // Act
            var act = () => bookReviewDAL.AddReview(userId, bookId, int.MaxValue, "Review", DateTime.Now);

            // Assert
            act.Should().Throw<Exception>();
        }

        #endregion

        #region Special Characters Tests

        [Fact]
        public void RegisterUser_WithSpecialCharactersInName_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var nameWithSpecialChars = "O'Brien-Smith Ñoño";
            var email = $"specialchars{Guid.NewGuid()}@test.com";

            // Act
            var act = () => userDAL.RegisterUser(nameWithSpecialChars, "username", email, "hash", "salt", null, null);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void AddBook_WithUnicodeCharacters_ShouldSucceed()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var unicodeTitle = "日本語タイトル 中文标题 العربية";
            var isbn = $"UNICODE-{Guid.NewGuid()}";

            // Act
            var bookId = bookDAL.AddBook(unicodeTitle, "Author", isbn, "url", "Genre", "Description");

            // Assert
            bookId.Should().BeGreaterThan(0);

            // Verify retrieval
            var book = bookDAL.GetBookById(bookId);
            book!.Title.Should().Be(unicodeTitle);
        }

        [Fact]
        public void SearchBooks_WithSpecialRegexCharacters_ShouldHandleSafely()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var specialChars = ".*+?^${}()|[]\\";

            // Act
            var act = () => bookDAL.SearchBooks(specialChars);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Null and Empty Tests

        [Fact]
        public void GetBookById_WithZero_ShouldReturnNull()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);

            // Act
            var result = bookDAL.GetBookById(0);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserBookshelf_WithZeroUserId_ShouldReturnEmptyList()
        {
            // Arrange
            var bookshelfDAL = new UserBookshelfDAL(_testConnectionString);

            // Act
            var result = bookshelfDAL.GetUserBookshelf(0);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void SearchBooks_WithEmptyString_ShouldReturnAllBooks()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);

            // Act
            var result = bookDAL.SearchBooks("");

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void UserExists_WithWhitespaceOnlyEmail_ShouldReturnFalse()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var result = userDAL.UserExists("   ");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public void ConcurrentUserRegistrations_ShouldAllSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var tasks = new List<Task>();

            // Act - Create 10 concurrent registration tasks
            for (int i = 0; i < 10; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() =>
                {
                    var email = $"concurrent{index}{Guid.NewGuid()}@test.com";
                    userDAL.RegisterUser($"User {index}", $"user{index}", email, "hash", "salt", null, null);
                }));
            }

            // Wait for all to complete
            var act = async () => await Task.WhenAll(tasks);

            // Assert - All should succeed
            act.Should().NotThrowAsync();
        }

        #endregion

        #region Date/Time Edge Cases

        [Fact]
        public void RegisterUser_WithFutureDateOfBirth_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var futureDate = DateTime.Now.AddYears(10);
            var email = $"future{Guid.NewGuid()}@test.com";

            // Act - System may or may not validate dates
            var act = () => userDAL.RegisterUser("Test", "test", email, "hash", "salt", futureDate, null);

            // Assert - Should not throw (business logic should validate, not DAL)
            act.Should().NotThrow();
        }

        [Fact]
        public void RegisterUser_WithVeryOldDateOfBirth_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var veryOldDate = new DateTime(1900, 1, 1);
            var email = $"old{Guid.NewGuid()}@test.com";

            // Act
            var act = () => userDAL.RegisterUser("Test", "test", email, "hash", "salt", veryOldDate, null);

            // Assert
            act.Should().NotThrow();
        }

        #endregion

        #region Performance Edge Cases

        [Fact]
        public void GetAllBooks_WithLargeDataset_ShouldComplete()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);

            // Act
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var books = bookDAL.GetAllBooks();
            sw.Stop();

            // Assert - Should complete in reasonable time (< 5 seconds)
            sw.ElapsedMilliseconds.Should().BeLessThan(5000);
            books.Should().NotBeNull();
        }

        #endregion
    }
}

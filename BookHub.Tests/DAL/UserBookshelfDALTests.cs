using BookHub.DAL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.DAL
{
    public class UserBookshelfDALTests
    {
        private readonly string _testConnectionString;

        public UserBookshelfDALTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void Constructor_WithValidConnectionString_ShouldSucceed()
        {
            // Arrange & Act
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);

            // Assert
            userBookshelfDAL.Should().NotBeNull();
        }

        [Fact]
        public void GetUserBookshelf_WithValidUserId_ShouldReturnBooksList()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var userId = 1;

            // Act
            var result = userBookshelfDAL.GetUserBookshelf(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<UserBookshelf>>();
        }

        [Fact]
        public void GetUserBookshelf_WithInvalidUserId_ShouldReturnEmptyList()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var invalidUserId = 999999;

            // Act
            var result = userBookshelfDAL.GetUserBookshelf(invalidUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetUserBookshelf_WithNegativeUserId_ShouldReturnEmptyList()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var negativeUserId = -1;

            // Act
            var result = userBookshelfDAL.GetUserBookshelf(negativeUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetUserBookshelf_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL("Invalid Connection String");
            var userId = 1;

            // Act
            var act = () => userBookshelfDAL.GetUserBookshelf(userId);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Database error occurred while retrieving user bookshelf:*");
        }

        [Fact]
        public void AddBookToUserBookshelf_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var userDAL = new UserDAL(_testConnectionString);

            // Create test data
            var userId = CreateTestUser(userDAL);
            var bookId = CreateTestBook(bookDAL);

            // Act
            var result = userBookshelfDAL.AddBookToUserBookshelf(userId, bookId, "Want to Read");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void AddBookToUserBookshelf_WithInvalidUserId_ShouldHandleGracefully()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var invalidUserId = 999999;
            var bookId = 1;

            // Act
            var act = () => userBookshelfDAL.AddBookToUserBookshelf(invalidUserId, bookId, "Want to Read");

            // Assert - Should throw or return false depending on implementation
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddBookToUserBookshelf_WithInvalidBookId_ShouldHandleGracefully()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var userId = 1;
            var invalidBookId = 999999;

            // Act
            var act = () => userBookshelfDAL.AddBookToUserBookshelf(userId, invalidBookId, "Want to Read");

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void AddBookToUserBookshelf_WithNullStatus_ShouldHandleGracefully()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var userId = 1;
            var bookId = 1;

            // Act
            var act = () => userBookshelfDAL.AddBookToUserBookshelf(userId, bookId, null!);

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void RemoveBookFromUserBookshelf_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var userDAL = new UserDAL(_testConnectionString);

            // Create test data
            var userId = CreateTestUser(userDAL);
            var bookId = CreateTestBook(bookDAL);
            userBookshelfDAL.AddBookToUserBookshelf(userId, bookId, "Want to Read");

            // Act
            var result = userBookshelfDAL.RemoveBookFromUserBookshelf(userId, bookId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RemoveBookFromUserBookshelf_WithNonExistentEntry_ShouldReturnFalse()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var userId = 999999;
            var bookId = 999999;

            // Act
            var result = userBookshelfDAL.RemoveBookFromUserBookshelf(userId, bookId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UpdateReadingStatus_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var userDAL = new UserDAL(_testConnectionString);

            // Create test data
            var userId = CreateTestUser(userDAL);
            var bookId = CreateTestBook(bookDAL);
            userBookshelfDAL.AddBookToUserBookshelf(userId, bookId, "Want to Read");

            // Act
            var result = userBookshelfDAL.UpdateReadingStatus(userId, bookId, "Currently Reading");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateReadingStatus_WithInvalidEntry_ShouldReturnFalse()
        {
            // Arrange
            var userBookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var userId = 999999;
            var bookId = 999999;

            // Act
            var result = userBookshelfDAL.UpdateReadingStatus(userId, bookId, "Read");

            // Assert
            result.Should().BeFalse();
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

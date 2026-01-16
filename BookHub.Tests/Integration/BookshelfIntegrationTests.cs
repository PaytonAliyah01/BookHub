using BookHub.DAL;
using BookHub.BLL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.Integration
{
    /// <summary>
    /// Integration tests for user bookshelf functionality
    /// </summary>
    public class BookshelfIntegrationTests
    {
        private readonly string _testConnectionString;

        public BookshelfIntegrationTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void AddBookToBookshelf_AndRetrieve_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var bookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var userBLL = new UserBLL(userDAL);
            var bookBLL = new BookBLL(bookDAL);
            var bookshelfBLL = new UserBookshelfBLL(bookshelfDAL, bookDAL);

            // Create test user
            var email = $"bookshelftest{Guid.NewGuid()}@test.com";
            userBLL.RegisterUser("Test User", "testuser", email, "password", null, null);
            var user = userBLL.GetUserByEmail(email);

            // Create test book
            var bookId = bookBLL.AddBook($"Test Book {Guid.NewGuid()}", "Author", "ISBN", "url", "Genre", "Desc");

            // Act - Add book to bookshelf
            var addResult = bookshelfBLL.AddBookToUserBookshelf(user!.UserId, bookId, "Want to Read");

            // Assert - Add succeeds
            addResult.Should().BeTrue();

            // Act - Retrieve bookshelf
            var bookshelf = bookshelfBLL.GetUserBookshelf(user.UserId);

            // Assert - Book is in bookshelf
            bookshelf.Should().NotBeEmpty();
            bookshelf.Should().Contain(b => b.BookId == bookId);
            var bookInShelf = bookshelf.First(b => b.BookId == bookId);
            bookInShelf.Status.Should().Be("Want to Read");
        }

        [Fact]
        public void UpdateReadingStatus_ShouldPersistChanges()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var bookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var bookshelfBLL = new UserBookshelfBLL(bookshelfDAL, bookDAL);

            // Create test user
            var email = $"statustest{Guid.NewGuid()}@test.com";
            var userBLL = new UserBLL(userDAL);
            userBLL.RegisterUser("Test User", "testuser", email, "password", null, null);
            var user = userBLL.GetUserByEmail(email);

            // Create test book
            var bookBLL = new BookBLL(bookDAL);
            var bookId = bookBLL.AddBook($"Test Book {Guid.NewGuid()}", "Author", "ISBN", "url", "Genre", "Desc");

            // Add book to bookshelf
            bookshelfBLL.AddBookToUserBookshelf(user!.UserId, bookId, "Want to Read");

            // Act - Update status
            var updateResult = bookshelfBLL.UpdateReadingStatus(user.UserId, bookId, "Currently Reading");

            // Assert - Update succeeds
            updateResult.Should().BeTrue();

            // Act - Retrieve bookshelf
            var bookshelf = bookshelfBLL.GetUserBookshelf(user.UserId);

            // Assert - Status updated
            var bookInShelf = bookshelf.First(b => b.BookId == bookId);
            bookInShelf.Status.Should().Be("Currently Reading");
        }

        [Fact]
        public void RemoveBookFromBookshelf_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var bookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var bookshelfBLL = new UserBookshelfBLL(bookshelfDAL, bookDAL);

            // Create test user
            var email = $"removetest{Guid.NewGuid()}@test.com";
            var userBLL = new UserBLL(userDAL);
            userBLL.RegisterUser("Test User", "testuser", email, "password", null, null);
            var user = userBLL.GetUserByEmail(email);

            // Create test book
            var bookBLL = new BookBLL(bookDAL);
            var bookId = bookBLL.AddBook($"Test Book {Guid.NewGuid()}", "Author", "ISBN", "url", "Genre", "Desc");

            // Add book to bookshelf
            bookshelfBLL.AddBookToUserBookshelf(user!.UserId, bookId, "Want to Read");

            // Act - Remove book from bookshelf
            var removeResult = bookshelfBLL.RemoveBookFromUserBookshelf(user.UserId, bookId);

            // Assert - Remove succeeds
            removeResult.Should().BeTrue();

            // Act - Retrieve bookshelf
            var bookshelf = bookshelfBLL.GetUserBookshelf(user.UserId);

            // Assert - Book no longer in bookshelf
            bookshelf.Should().NotContain(b => b.BookId == bookId);
        }

        [Fact]
        public void AddDuplicateBookToBookshelf_ShouldHandleGracefully()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var bookDAL = new BookDAL(_testConnectionString);
            var bookshelfDAL = new UserBookshelfDAL(_testConnectionString);
            var bookshelfBLL = new UserBookshelfBLL(bookshelfDAL, bookDAL);

            // Create test user
            var email = $"duptest{Guid.NewGuid()}@test.com";
            var userBLL = new UserBLL(userDAL);
            userBLL.RegisterUser("Test User", "testuser", email, "password", null, null);
            var user = userBLL.GetUserByEmail(email);

            // Create test book
            var bookBLL = new BookBLL(bookDAL);
            var bookId = bookBLL.AddBook($"Test Book {Guid.NewGuid()}", "Author", "ISBN", "url", "Genre", "Desc");

            // Add book first time
            bookshelfBLL.AddBookToUserBookshelf(user!.UserId, bookId, "Want to Read");

            // Act - Try to add same book again
            var secondAdd = () => bookshelfBLL.AddBookToUserBookshelf(user.UserId, bookId, "Currently Reading");

            // Assert - Should throw or handle gracefully
            secondAdd.Should().Throw<Exception>();
        }
    }
}

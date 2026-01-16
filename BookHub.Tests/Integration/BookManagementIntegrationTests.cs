using BookHub.DAL;
using BookHub.BLL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.Integration
{
    /// <summary>
    /// Integration tests for the complete book management workflow
    /// </summary>
    public class BookManagementIntegrationTests
    {
        private readonly string _testConnectionString;

        public BookManagementIntegrationTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void AddBookAndRetrieve_ShouldSucceed()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var bookBLL = new BookBLL(bookDAL);

            var title = $"Integration Book {Guid.NewGuid()}";
            var author = "Integration Author";
            var isbn = $"ISBN-{Guid.NewGuid()}";
            var genre = "Science Fiction";
            var description = "A test book for integration testing";

            // Act - Add book
            var bookId = bookBLL.AddBook(title, author, isbn, "http://test.com/cover.jpg", genre, description);

            // Assert - Book ID returned
            bookId.Should().BeGreaterThan(0);

            // Act - Retrieve book by ID
            var retrievedBook = bookBLL.GetBookById(bookId);

            // Assert - Book details match
            retrievedBook.Should().NotBeNull();
            retrievedBook!.Title.Should().Be(title);
            retrievedBook.Author.Should().Be(author);
            retrievedBook.ISBN.Should().Be(isbn);
            retrievedBook.Genre.Should().Be(genre);
        }

        [Fact]
        public void SearchBooksAfterAddition_ShouldFindBook()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var bookBLL = new BookBLL(bookDAL);

            var uniqueKeyword = $"UniqueBook{Guid.NewGuid().ToString().Substring(0, 8)}";
            var title = $"{uniqueKeyword} Test Book";

            // Act - Add book
            bookBLL.AddBook(title, "Author", "ISBN123", "url", "Genre", "Description");

            // Act - Search for book
            var searchResults = bookBLL.SearchBooks(uniqueKeyword);

            // Assert - Book found in search
            searchResults.Should().NotBeEmpty();
            searchResults.Should().Contain(b => b.Title.Contains(uniqueKeyword));
        }

        [Fact]
        public void UpdateBookDetails_ShouldPersistChanges()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var bookBLL = new BookBLL(bookDAL);

            var originalTitle = $"Original Title {Guid.NewGuid()}";
            var updatedTitle = $"Updated Title {Guid.NewGuid()}";

            // Act - Add book
            var bookId = bookBLL.AddBook(originalTitle, "Author", "ISBN", "url", "Genre", "Desc");

            // Act - Update book
            var updateResult = bookBLL.UpdateBook(bookId, updatedTitle, "New Author", "NEW-ISBN", "newurl", "New Genre", "New Desc");

            // Assert - Update succeeds
            updateResult.Should().BeTrue();

            // Act - Retrieve updated book
            var updatedBook = bookBLL.GetBookById(bookId);

            // Assert - Changes persisted
            updatedBook.Should().NotBeNull();
            updatedBook!.Title.Should().Be(updatedTitle);
            updatedBook.Author.Should().Be("New Author");
        }

        [Fact]
        public void DeleteBook_ShouldRemoveFromDatabase()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var bookBLL = new BookBLL(bookDAL);

            var title = $"Book To Delete {Guid.NewGuid()}";

            // Act - Add book
            var bookId = bookBLL.AddBook(title, "Author", "ISBN", "url", "Genre", "Desc");

            // Act - Delete book
            var deleteResult = bookBLL.DeleteBook(bookId);

            // Assert - Delete succeeds
            deleteResult.Should().BeTrue();

            // Act - Try to retrieve deleted book
            var deletedBook = bookBLL.GetBookById(bookId);

            // Assert - Book no longer exists
            deletedBook.Should().BeNull();
        }
    }
}

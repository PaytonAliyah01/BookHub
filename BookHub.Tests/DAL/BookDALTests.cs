using BookHub.DAL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.DAL
{
    public class BookDALTests
    {
        private readonly string _testConnectionString;

        public BookDALTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void Constructor_WithValidConnectionString_ShouldSucceed()
        {
            // Arrange & Act
            var bookDAL = new BookDAL(_testConnectionString);

            // Assert
            bookDAL.Should().NotBeNull();
        }

        [Fact]
        public void GetAllBooks_WithValidConnection_ShouldReturnBooksList()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);

            // Act
            var result = bookDAL.GetAllBooks();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Book>>();
        }

        [Fact]
        public void GetAllBooks_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookDAL = new BookDAL("Invalid Connection String");

            // Act
            var act = () => bookDAL.GetAllBooks();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Database error occurred while retrieving books:*");
        }

        [Fact]
        public void GetBookById_WithValidBookId_ShouldReturnBook()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var books = bookDAL.GetAllBooks();
            
            if (books.Count > 0)
            {
                var validBookId = books[0].BookId;

                // Act
                var result = bookDAL.GetBookById(validBookId);

                // Assert
                result.Should().NotBeNull();
                result!.BookId.Should().Be(validBookId);
            }
        }

        [Fact]
        public void GetBookById_WithInvalidBookId_ShouldReturnNull()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var invalidBookId = 999999;

            // Act
            var result = bookDAL.GetBookById(invalidBookId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetBookById_WithNegativeBookId_ShouldReturnNull()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var negativeBookId = -1;

            // Act
            var result = bookDAL.GetBookById(negativeBookId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetBookById_WithZeroBookId_ShouldReturnNull()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var zeroBookId = 0;

            // Act
            var result = bookDAL.GetBookById(zeroBookId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetBookById_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookDAL = new BookDAL("Invalid Connection String");
            var bookId = 1;

            // Act
            var act = () => bookDAL.GetBookById(bookId);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Database error occurred while retrieving book:*");
        }

        [Fact]
        public void SearchBooks_WithValidKeyword_ShouldReturnMatchingBooks()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var keyword = "a"; // Common letter

            // Act
            var result = bookDAL.SearchBooks(keyword);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Book>>();
        }

        [Fact]
        public void SearchBooks_WithEmptyKeyword_ShouldReturnAllBooks()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var emptyKeyword = "";

            // Act
            var result = bookDAL.SearchBooks(emptyKeyword);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Book>>();
        }

        [Fact]
        public void SearchBooks_WithNonExistentKeyword_ShouldReturnEmptyList()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var nonExistentKeyword = "XYZQWERTY12345NONEXISTENT";

            // Act
            var result = bookDAL.SearchBooks(nonExistentKeyword);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void SearchBooks_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookDAL = new BookDAL("Invalid Connection String");
            var keyword = "test";

            // Act
            var act = () => bookDAL.SearchBooks(keyword);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Database error occurred while searching books:*");
        }

        [Fact]
        public void AddBook_WithValidBook_ShouldReturnBookId()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);
            var title = "Test Book " + Guid.NewGuid();
            var author = "Test Author";
            var isbn = "TEST-" + Guid.NewGuid();
            var coverUrl = "http://test.com/cover.jpg";
            var genre = "Fiction";
            var description = "Test description";

            // Act
            var result = bookDAL.AddBook(title, author, isbn, coverUrl, genre, description);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void AddBook_WithNullTitle_ShouldThrowArgumentException()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);

            // Act
            var act = () => bookDAL.AddBook(null!, "Author", "ISBN", "url", "Genre", "Desc");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }

        [Fact]
        public void AddBook_WithEmptyAuthor_ShouldThrowArgumentException()
        {
            // Arrange
            var bookDAL = new BookDAL(_testConnectionString);

            // Act
            var act = () => bookDAL.AddBook("Title", "", "ISBN", "url", "Genre", "Desc");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }
    }
}

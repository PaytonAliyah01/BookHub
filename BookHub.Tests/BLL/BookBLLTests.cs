using BookHub.BLL;
using BookHub.DAL;
using BookHub.DAL.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookHub.Tests.BLL
{
    public class BookBLLTests
    {
        private readonly Mock<IBookDAL> _mockBookDAL;
        private readonly BookBLL _bookBLL;

        public BookBLLTests()
        {
            _mockBookDAL = new Mock<IBookDAL>();
            _bookBLL = new BookBLL(_mockBookDAL.Object);
        }

        [Fact]
        public void Constructor_WithValidDAL_ShouldSucceed()
        {
            // Arrange & Act
            var bookBLL = new BookBLL(_mockBookDAL.Object);

            // Assert
            bookBLL.Should().NotBeNull();
        }

        [Fact]
        public void GetAllBooks_WithValidData_ShouldReturnBookDtoList()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { BookId = 1, Title = "Book 1", Author = "Author 1", ISBN = "ISBN1", Genre = "Fiction" },
                new Book { BookId = 2, Title = "Book 2", Author = "Author 2", ISBN = "ISBN2", Genre = "Non-Fiction" }
            };

            _mockBookDAL.Setup(x => x.GetAllBooks())
                .Returns(books);

            // Act
            var result = _bookBLL.GetAllBooks();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Title.Should().Be("Book 1");
        }

        [Fact]
        public void GetAllBooks_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            _mockBookDAL.Setup(x => x.GetAllBooks())
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _bookBLL.GetAllBooks();

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to retrieve books. Please try again later.");
        }

        [Fact]
        public void GetBookById_WithValidBookId_ShouldReturnBookDto()
        {
            // Arrange
            var bookId = 1;
            var book = new Book
            {
                BookId = bookId,
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "TEST123",
                Genre = "Fiction",
                CoverUrl = "http://test.com/cover.jpg",
                Description = "Test description"
            };

            _mockBookDAL.Setup(x => x.GetBookById(bookId))
                .Returns(book);

            // Act
            var result = _bookBLL.GetBookById(bookId);

            // Assert
            result.Should().NotBeNull();
            result!.BookId.Should().Be(bookId);
            result.Title.Should().Be("Test Book");
        }

        [Fact]
        public void GetBookById_WithInvalidBookId_ShouldReturnNull()
        {
            // Arrange
            var bookId = 999999;

            _mockBookDAL.Setup(x => x.GetBookById(bookId))
                .Returns((Book?)null);

            // Act
            var result = _bookBLL.GetBookById(bookId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetBookById_WithZeroOrNegativeBookId_ShouldReturnNull()
        {
            // Arrange
            var bookId = 0;

            // Act
            var result = _bookBLL.GetBookById(bookId);

            // Assert
            result.Should().BeNull();
            _mockBookDAL.Verify(x => x.GetBookById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetBookById_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var bookId = 1;

            _mockBookDAL.Setup(x => x.GetBookById(bookId))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _bookBLL.GetBookById(bookId);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to retrieve book. Please try again later.");
        }

        [Fact]
        public void SearchBooks_WithValidKeyword_ShouldReturnMatchingBooks()
        {
            // Arrange
            var keyword = "test";
            var books = new List<Book>
            {
                new Book { BookId = 1, Title = "Test Book", Author = "Author", ISBN = "ISBN1", Genre = "Fiction" }
            };

            _mockBookDAL.Setup(x => x.SearchBooks(keyword))
                .Returns(books);

            // Act
            var result = _bookBLL.SearchBooks(keyword);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public void SearchBooks_WithEmptyKeyword_ShouldReturnAllBooks()
        {
            // Arrange
            var keyword = "";
            var books = new List<Book>
            {
                new Book { BookId = 1, Title = "Book 1", Author = "Author 1", ISBN = "ISBN1", Genre = "Fiction" },
                new Book { BookId = 2, Title = "Book 2", Author = "Author 2", ISBN = "ISBN2", Genre = "Non-Fiction" }
            };

            _mockBookDAL.Setup(x => x.SearchBooks(keyword))
                .Returns(books);

            // Act
            var result = _bookBLL.SearchBooks(keyword);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void SearchBooks_WithNonExistentKeyword_ShouldReturnEmptyList()
        {
            // Arrange
            var keyword = "nonexistent";

            _mockBookDAL.Setup(x => x.SearchBooks(keyword))
                .Returns(new List<Book>());

            // Act
            var result = _bookBLL.SearchBooks(keyword);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void SearchBooks_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var keyword = "test";

            _mockBookDAL.Setup(x => x.SearchBooks(keyword))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _bookBLL.SearchBooks(keyword);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to search books. Please try again later.");
        }

        [Fact]
        public void AddBook_WithValidData_ShouldReturnBookId()
        {
            // Arrange
            var title = "New Book";
            var author = "New Author";
            var isbn = "NEW123";
            var coverUrl = "http://test.com/cover.jpg";
            var genre = "Fiction";
            var description = "New description";
            var expectedBookId = 1;

            _mockBookDAL.Setup(x => x.AddBook(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(expectedBookId);

            // Act
            var result = _bookBLL.AddBook(title, author, isbn, coverUrl, genre, description);

            // Assert
            result.Should().Be(expectedBookId);
        }

        [Fact]
        public void AddBook_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var title = "New Book";
            var author = "New Author";
            var isbn = "NEW123";
            var coverUrl = "http://test.com/cover.jpg";
            var genre = "Fiction";
            var description = "New description";

            _mockBookDAL.Setup(x => x.AddBook(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _bookBLL.AddBook(title, author, isbn, coverUrl, genre, description);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("*Unable to add book*");
        }

        [Fact]
        public void UpdateBook_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var bookId = 1;
            var title = "Updated Book";
            var author = "Updated Author";
            var isbn = "UPD123";
            var coverUrl = "http://test.com/cover.jpg";
            var genre = "Fiction";
            var description = "Updated description";

            _mockBookDAL.Setup(x => x.UpdateBook(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // Act
            var result = _bookBLL.UpdateBook(bookId, title, author, isbn, coverUrl, genre, description);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateBook_WithInvalidBookId_ShouldReturnFalse()
        {
            // Arrange
            var bookId = 999999;
            var title = "Updated Book";
            var author = "Updated Author";
            var isbn = "UPD123";
            var coverUrl = "http://test.com/cover.jpg";
            var genre = "Fiction";
            var description = "Updated description";

            _mockBookDAL.Setup(x => x.UpdateBook(bookId, title, author, isbn, coverUrl, genre, description))
                .Returns(false);

            // Act
            var result = _bookBLL.UpdateBook(bookId, title, author, isbn, coverUrl, genre, description);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DeleteBook_WithValidBookId_ShouldReturnTrue()
        {
            // Arrange
            var bookId = 1;

            _mockBookDAL.Setup(x => x.DeleteBook(bookId))
                .Returns(true);

            // Act
            var result = _bookBLL.DeleteBook(bookId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void DeleteBook_WithInvalidBookId_ShouldReturnFalse()
        {
            // Arrange
            var bookId = 999999;

            _mockBookDAL.Setup(x => x.DeleteBook(bookId))
                .Returns(false);

            // Act
            var result = _bookBLL.DeleteBook(bookId);

            // Assert
            result.Should().BeFalse();
        }
    }
}

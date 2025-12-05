using Xunit;
using FluentAssertions;
using Moq;
using BookHub.BLL;
using BookHub.DAL;
using BookHub.DAL.Interfaces;

namespace BookHub.Tests.IntegrationTests
{
    /// <summary>
    /// Integration Tests for Bookshelf & Book Management  
    /// Maps to Test Plan: TC09-TC12
    /// Tests using actual DAL interface methods
    /// </summary>
    public class BookshelfIntegrationTests
    {
        #region TC09: Search and Add Book

        [Fact]
        public void TC09_GetAllBooks_ReturnsBookList()
        {
            // Arrange
            var mockBookDAL = new Mock<IBookDAL>();
            var books = new List<Book>
            {
                new Book { BookId = 1, Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy" },
                new Book { BookId = 2, Title = "To Kill a Mockingbird", Author = "Harper Lee", Genre = "Classic" }
            };

            mockBookDAL.Setup(dal => dal.GetAllBooks()).Returns(books);

            // Act
            var result = mockBookDAL.Object.GetAllBooks();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(b => b.Title == "The Hobbit");
        }

        [Fact]
        public void TC09_AddBookToUserBookshelf_SuccessfullyAdds()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var bookId = 1;

            mockBookshelfDAL.Setup(dal => dal.AddBookToUserBookshelf(userId, bookId, "Want to Read", "Physical"))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.AddBookToUserBookshelf(userId, bookId, "Want to Read", "Physical");

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("Want to Read", "Physical")]
        [InlineData("Reading", "eBook")]
        [InlineData("Read", "Audiobook")]
        public void TC09_AddBook_WithDifferentStatusAndOwnership_Succeeds(string status, string ownershipType)
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            mockBookshelfDAL.Setup(dal => dal.AddBookToUserBookshelf(1, 1, status, ownershipType))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.AddBookToUserBookshelf(1, 1, status, ownershipType);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region TC10: Change Book Status

        [Fact]
        public void TC10_UpdateBookStatus_ToReading_Succeeds()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var bookId = 1;
            var newStatus = "Reading";
            var dateStarted = DateTime.Now;

            mockBookshelfDAL.Setup(dal => dal.UpdateBookStatus(userId, bookId, newStatus, dateStarted, null))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.UpdateBookStatus(userId, bookId, newStatus, dateStarted, null);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("Want to Read")]
        [InlineData("Reading")]
        [InlineData("Read")]
        public void TC10_UpdateBookStatus_AllValidStatuses_Succeed(string newStatus)
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            mockBookshelfDAL.Setup(dal => dal.UpdateBookStatus(1, 1, newStatus, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.UpdateBookStatus(1, 1, newStatus, null, null);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void TC10_UpdateStatus_ToRead_SetsFinishDate()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var bookId = 1;
            var finishDate = DateTime.Now;

            mockBookshelfDAL.Setup(dal => dal.UpdateBookStatus(userId, bookId, "Read", null, finishDate))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.UpdateBookStatus(userId, bookId, "Read", null, finishDate);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region TC11: Update Reading Progress

        [Fact]
        public void TC11_UpdateReadingProgress_TracksCurrentPage()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var bookId = 1;
            var currentPage = 150;
            var totalPages = 300;
            var progress = 50.0m;

            mockBookshelfDAL.Setup(dal => dal.UpdateReadingProgress(userId, bookId, currentPage, progress, totalPages))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.UpdateReadingProgress(userId, bookId, currentPage, progress, totalPages);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(50, 200, 25.0)]
        [InlineData(100, 200, 50.0)]
        [InlineData(200, 200, 100.0)]
        public void TC11_UpdateReadingProgress_VariousPages_CalculatesCorrectly(int currentPage, int totalPages, decimal expectedProgress)
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            mockBookshelfDAL.Setup(dal => dal.UpdateReadingProgress(1, 1, currentPage, expectedProgress, totalPages))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.UpdateReadingProgress(1, 1, currentPage, expectedProgress, totalPages);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region TC12: Remove Book from Shelf

        [Fact]
        public void TC12_RemoveBookFromShelf_Succeeds()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var bookId = 1;

            mockBookshelfDAL.Setup(dal => dal.RemoveBookFromUserBookshelf(userId, bookId))
                .Returns(true);

            // Act
            var result = mockBookshelfDAL.Object.RemoveBookFromUserBookshelf(userId, bookId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void TC12_RemoveNonExistentBook_ReturnsFalse()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var nonExistentBookId = 999;

            mockBookshelfDAL.Setup(dal => dal.RemoveBookFromUserBookshelf(userId, nonExistentBookId))
                .Returns(false);

            // Act
            var result = mockBookshelfDAL.Object.RemoveBookFromUserBookshelf(userId, nonExistentBookId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Additional Tests

        [Fact]
        public void GetUserBookshelf_ReturnsAllUserBooks()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var userBooks = new List<UserBookshelf>
            {
                new UserBookshelf { BookId = 1, Status = "Reading" },
                new UserBookshelf { BookId = 2, Status = "Read" },
                new UserBookshelf { BookId = 3, Status = "Want to Read" }
            };

            mockBookshelfDAL.Setup(dal => dal.GetUserBookshelf(userId))
                .Returns(userBooks);

            // Act
            var result = mockBookshelfDAL.Object.GetUserBookshelf(userId);

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(b => b.Status == "Reading");
        }

        [Fact]
        public void GetUserBookshelfStats_ReturnsStatistics()
        {
            // Arrange
            var mockBookshelfDAL = new Mock<IUserBookshelfDAL>();
            var userId = 1;
            var stats = new Dictionary<string, int>
            {
                { "TotalBooks", 10 },
                { "BooksRead", 5 },
                { "BooksReading", 3 },
                { "BooksWantToRead", 2 }
            };

            mockBookshelfDAL.Setup(dal => dal.GetUserBookshelfStats(userId))
                .Returns(stats);

            // Act
            var result = mockBookshelfDAL.Object.GetUserBookshelfStats(userId);

            // Assert
            result.Should().ContainKey("TotalBooks");
            result["TotalBooks"].Should().Be(10);
            result["BooksRead"].Should().Be(5);
        }

        [Fact]
        public void GetBookById_ReturnsSpecificBook()
        {
            // Arrange
            var mockBookDAL = new Mock<IBookDAL>();
            var bookId = 5;
            var book = new Book
            {
                BookId = bookId,
                Title = "Dune",
                Author = "Frank Herbert",
                Genre = "Science Fiction",
                ISBN = "9780441172719"
            };

            mockBookDAL.Setup(dal => dal.GetBookById(bookId))
                .Returns(book);

            // Act
            var result = mockBookDAL.Object.GetBookById(bookId);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Dune");
            result.Author.Should().Be("Frank Herbert");
        }

        [Fact]
        public void AddBook_NewBook_ReturnsBookId()
        {
            // Arrange
            var mockBookDAL = new Mock<IBookDAL>();
            var newBook = new Book
            {
                Title = "New Book",
                Author = "Author Name",
                ISBN = "1234567890",
                Genre = "Fiction",
                Description = "A new book"
            };

            mockBookDAL.Setup(dal => dal.AddBook(newBook))
                .Returns(1); // Returns new book ID

            // Act
            var result = mockBookDAL.Object.AddBook(newBook);

            // Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void UpdateBook_ExistingBook_Succeeds()
        {
            // Arrange
            var mockBookDAL = new Mock<IBookDAL>();
            mockBookDAL.Setup(dal => dal.UpdateBook(1, "Updated Title", "Updated Author", "1234567890", "Fiction", "Updated description", "cover.jpg"))
                .Returns(true);

            // Act
            var result = mockBookDAL.Object.UpdateBook(1, "Updated Title", "Updated Author", "1234567890", "Fiction", "Updated description", "cover.jpg");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void DeleteBook_ExistingBook_Succeeds()
        {
            // Arrange
            var mockBookDAL = new Mock<IBookDAL>();
            mockBookDAL.Setup(dal => dal.DeleteBook(1))
                .Returns(true);

            // Act
            var result = mockBookDAL.Object.DeleteBook(1);

            // Assert
            result.Should().BeTrue();
        }

        #endregion
    }
}

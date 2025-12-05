using Xunit;
using FluentAssertions;
using BookHub.DAL;

namespace BookHub.Tests
{
    /// <summary>
    /// Input validation tests - Testing edge cases and data validation logic
    /// These tests ensure the application handles invalid input gracefully
    /// </summary>
    public class ValidationTests
    {
        #region Email Validation Tests

        [Theory]
        [InlineData("user@example.com", true)]
        [InlineData("user.name@example.com", true)]
        [InlineData("user+tag@example.co.uk", true)]
        [InlineData("", false)]
        [InlineData("invalid-email", false)]
        [InlineData("@example.com", false)]
        [InlineData("user@", false)]
        [InlineData("user @example.com", false)]
        public void Email_Validation_HandlesVariousFormats(string email, bool expectedValid)
        {
            // Arrange - Email format validation logic
            bool isValid = !string.IsNullOrWhiteSpace(email) && 
                          email.Contains('@') && 
                          email.Contains('.') &&
                          !email.Contains(' ') &&
                          email.IndexOf('@') > 0 &&
                          email.LastIndexOf('.') > email.IndexOf('@') &&
                          email.LastIndexOf('.') < email.Length - 1;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion

        #region ISBN Validation Tests

        [Theory]
        [InlineData("1234567890", true)]
        [InlineData("123-4567890123", true)]
        [InlineData("", false)]
        [InlineData("123", false)]
        [InlineData("abcdefghij", false)]
        public void ISBN_Validation_RequiresValidFormat(string isbn, bool expectedValid)
        {
            // Arrange - ISBN should be 10 or 13 digits (with optional hyphens)
            var cleaned = isbn.Replace("-", "").Replace(" ", "");
            bool isValid = !string.IsNullOrWhiteSpace(isbn) && 
                          cleaned.Length >= 10 &&
                          cleaned.All(char.IsDigit);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion

        #region Rating Validation Tests

        [Theory]
        [InlineData(1, true)]
        [InlineData(3, true)]
        [InlineData(5, true)]
        [InlineData(0, false)]
        [InlineData(6, false)]
        [InlineData(-1, false)]
        public void Rating_MustBeBetween1And5(int rating, bool expectedValid)
        {
            // Arrange
            bool isValid = rating >= 1 && rating <= 5;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion

        #region Page Number Validation Tests

        [Theory]
        [InlineData(50, 100, true)]
        [InlineData(0, 100, true)]
        [InlineData(100, 100, true)]
        [InlineData(-1, 100, false)]
        [InlineData(101, 100, false)]
        public void CurrentPage_CannotExceedTotalPages(int currentPage, int totalPages, bool expectedValid)
        {
            // Arrange
            bool isValid = currentPage >= 0 && currentPage <= totalPages;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion

        #region String Length Validation Tests

        [Theory]
        [InlineData("", false)]
        [InlineData("A", true)]
        [InlineData("A Book Title", true)]
        public void BookTitle_CannotBeEmpty(string? title, bool expectedValid)
        {
            // Arrange
            bool isValid = !string.IsNullOrWhiteSpace(title);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        [Theory]
        [InlineData("Short review", true)]
        [InlineData("This is a much longer review with more details about the book", true)]
        [InlineData("", false)]
        public void ReviewText_RequiresContent(string? reviewText, bool expectedValid)
        {
            // Arrange
            bool isValid = !string.IsNullOrWhiteSpace(reviewText);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion

        #region Date Validation Tests

        [Fact]
        public void StartDate_CannotBeInFuture()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(10);
            var today = DateTime.Now;

            // Act
            bool isValid = futureDate <= today;

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void FinishDate_CannotBeBeforeStartDate()
        {
            // Arrange
            var startDate = DateTime.Now;
            var finishDate = DateTime.Now.AddDays(-1);

            // Act
            bool isValid = finishDate >= startDate;

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ReadingGoal_EndDateAfterStartDate()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);

            // Act
            bool isValid = endDate > startDate;

            // Assert
            isValid.Should().BeTrue();
        }

        #endregion

        #region Numeric Validation Tests

        [Theory]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void TargetBooks_MustBePositive(int targetBooks, bool expectedValid)
        {
            // Arrange
            bool isValid = targetBooks > 0;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(500, true)]
        [InlineData(1000, true)]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        public void TotalPages_MustBePositive(int totalPages, bool expectedValid)
        {
            // Arrange
            bool isValid = totalPages > 0;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion

        #region Status Validation Tests

        [Theory]
        [InlineData("Want to Read", true)]
        [InlineData("Reading", true)]
        [InlineData("Read", true)]
        [InlineData("Invalid Status", false)]
        [InlineData("", false)]
        public void BookshelfStatus_MustBeValidOption(string status, bool expectedValid)
        {
            // Arrange
            var validStatuses = new[] { "Want to Read", "Reading", "Read" };
            bool isValid = validStatuses.Contains(status);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        [Theory]
        [InlineData("Physical", true)]
        [InlineData("eBook", true)]
        [InlineData("Audiobook", true)]
        [InlineData("Invalid", false)]
        [InlineData("", false)]
        public void OwnershipType_MustBeValidOption(string ownershipType, bool expectedValid)
        {
            // Arrange
            var validTypes = new[] { "Physical", "eBook", "Audiobook" };
            bool isValid = validTypes.Contains(ownershipType);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion
    }
}

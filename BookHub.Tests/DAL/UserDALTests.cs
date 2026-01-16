using BookHub.DAL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.DAL
{
    public class UserDALTests
    {
        private readonly string _testConnectionString;

        public UserDALTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void Constructor_WithValidConnectionString_ShouldSucceed()
        {
            // Arrange & Act
            var userDAL = new UserDAL(_testConnectionString);

            // Assert
            userDAL.Should().NotBeNull();
        }

        [Fact]
        public void UserExists_WithExistingEmail_ShouldReturnTrue()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            
            // First, register a test user
            var testEmail = $"test{Guid.NewGuid()}@test.com";
            userDAL.RegisterUser("Test User", "testuser", testEmail, "hashedpass", "salt", null, null);

            // Act
            var result = userDAL.UserExists(testEmail);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UserExists_WithNonExistentEmail_ShouldReturnFalse()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var nonExistentEmail = $"nonexistent{Guid.NewGuid()}@test.com";

            // Act
            var result = userDAL.UserExists(nonExistentEmail);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UserExists_WithNullEmail_ShouldReturnFalse()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var result = userDAL.UserExists(null!);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UserExists_WithEmptyEmail_ShouldReturnFalse()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var result = userDAL.UserExists("");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UserExists_WithWhitespaceEmail_ShouldReturnFalse()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var result = userDAL.UserExists("   ");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UserExists_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var userDAL = new UserDAL("Invalid Connection String");
            var email = "test@test.com";

            // Act
            var act = () => userDAL.UserExists(email);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Database error occurred while checking user existence:*");
        }

        [Fact]
        public void RegisterUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var name = "Test User";
            var username = $"testuser{Guid.NewGuid()}";
            var email = $"test{Guid.NewGuid()}@test.com";
            var hashedPassword = "hashedpassword123";
            var salt = "saltsalt";
            var dateOfBirth = new DateTime(1990, 1, 1);
            var gender = "Male";

            // Act
            var act = () => userDAL.RegisterUser(name, username, email, hashedPassword, salt, dateOfBirth, gender);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void RegisterUser_WithNullName_ShouldThrowArgumentException()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var act = () => userDAL.RegisterUser(null!, "username", "email@test.com", "hash", "salt", null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }

        [Fact]
        public void RegisterUser_WithEmptyUsername_ShouldThrowArgumentException()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var act = () => userDAL.RegisterUser("Name", "", "email@test.com", "hash", "salt", null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }

        [Fact]
        public void RegisterUser_WithNullEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var act = () => userDAL.RegisterUser("Name", "username", null!, "hash", "salt", null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }

        [Fact]
        public void RegisterUser_WithNullHashedPassword_ShouldThrowArgumentException()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var act = () => userDAL.RegisterUser("Name", "username", "email@test.com", null!, "salt", null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }

        [Fact]
        public void RegisterUser_WithNullSalt_ShouldThrowArgumentException()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var act = () => userDAL.RegisterUser("Name", "username", "email@test.com", "hash", null!, null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("All required parameters must be provided and cannot be empty.");
        }

        [Fact]
        public void ValidateUser_WithValidCredentials_ShouldReturnUser()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var email = $"test{Guid.NewGuid()}@test.com";
            var password = "testpassword";
            
            // Register user first
            var salt = Guid.NewGuid().ToString();
            var hashedPassword = userDAL.HashPassword(password, salt);
            userDAL.RegisterUser("Test User", "testuser", email, hashedPassword, salt, null, null);

            // Act
            var result = userDAL.ValidateUser(email, password);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public void ValidateUser_WithInvalidEmail_ShouldReturnNull()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var email = $"nonexistent{Guid.NewGuid()}@test.com";
            var password = "password";

            // Act
            var result = userDAL.ValidateUser(email, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateUser_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var email = $"test{Guid.NewGuid()}@test.com";
            var password = "correctpassword";
            var wrongPassword = "wrongpassword";
            
            // Register user first
            var salt = Guid.NewGuid().ToString();
            var hashedPassword = userDAL.HashPassword(password, salt);
            userDAL.RegisterUser("Test User", "testuser", email, hashedPassword, salt, null, null);

            // Act
            var result = userDAL.ValidateUser(email, wrongPassword);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void HashPassword_WithValidInputs_ShouldReturnHashedString()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var password = "testpassword";
            var salt = "testsalt";

            // Act
            var result = userDAL.HashPassword(password, salt);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().NotBe(password); // Should be hashed
        }

        [Fact]
        public void HashPassword_WithSameInputs_ShouldReturnSameHash()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var password = "testpassword";
            var salt = "testsalt";

            // Act
            var hash1 = userDAL.HashPassword(password, salt);
            var hash2 = userDAL.HashPassword(password, salt);

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void HashPassword_WithDifferentSalts_ShouldReturnDifferentHashes()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var password = "testpassword";
            var salt1 = "salt1";
            var salt2 = "salt2";

            // Act
            var hash1 = userDAL.HashPassword(password, salt1);
            var hash2 = userDAL.HashPassword(password, salt2);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void GetUserByEmail_WithValidEmail_ShouldReturnUser()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var email = $"test{Guid.NewGuid()}@test.com";
            userDAL.RegisterUser("Test User", "testuser", email, "hash", "salt", null, null);

            // Act
            var result = userDAL.GetUserByEmail(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public void GetUserByEmail_WithNonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var email = $"nonexistent{Guid.NewGuid()}@test.com";

            // Act
            var result = userDAL.GetUserByEmail(email);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserByEmail_WithNullEmail_ShouldReturnNull()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);

            // Act
            var result = userDAL.GetUserByEmail(null!);

            // Assert
            result.Should().BeNull();
        }
    }
}

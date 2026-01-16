using BookHub.BLL;
using BookHub.DAL;
using BookHub.DAL.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookHub.Tests.BLL
{
    public class UserBLLTests
    {
        private readonly Mock<IUserDAL> _mockUserDAL;
        private readonly UserBLL _userBLL;

        public UserBLLTests()
        {
            _mockUserDAL = new Mock<IUserDAL>();
            _userBLL = new UserBLL(_mockUserDAL.Object);
        }

        [Fact]
        public void Constructor_WithValidDAL_ShouldSucceed()
        {
            // Arrange & Act
            var userBLL = new UserBLL(_mockUserDAL.Object);

            // Assert
            userBLL.Should().NotBeNull();
        }

        [Fact]
        public void RegisterUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var name = "Test User";
            var username = "testuser";
            var email = "test@test.com";
            var password = "password123";
            var dateOfBirth = new DateTime(1990, 1, 1);
            var gender = "Male";

            _mockUserDAL.Setup(x => x.UserExists(email)).Returns(false);
            _mockUserDAL.Setup(x => x.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("hashedpassword");
            _mockUserDAL.Setup(x => x.RegisterUser(name, username, email, It.IsAny<string>(), It.IsAny<string>(), dateOfBirth, gender));

            // Act
            var act = () => _userBLL.RegisterUser(name, username, email, password, dateOfBirth, gender);

            // Assert
            act.Should().NotThrow();
            _mockUserDAL.Verify(x => x.RegisterUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<string>(), dateOfBirth, gender), Times.Once);
        }

        [Fact]
        public void RegisterUser_WithExistingEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var name = "Test User";
            var username = "testuser";
            var email = "existing@test.com";
            var password = "password123";

            _mockUserDAL.Setup(x => x.UserExists(email)).Returns(true);

            // Act
            var act = () => _userBLL.RegisterUser(name, username, email, password, null, null);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("User with this email already exists.");
        }

        [Fact]
        public void RegisterUser_WithNullName_ShouldThrowArgumentException()
        {
            // Arrange
            var username = "testuser";
            var email = "test@test.com";
            var password = "password123";

            // Act
            var act = () => _userBLL.RegisterUser("", username, email, password, null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Name, username, email, and password cannot be null or empty.");
        }

        [Fact]
        public void RegisterUser_WithEmptyUsername_ShouldThrowArgumentException()
        {
            // Arrange
            var name = "Test User";
            var email = "test@test.com";
            var password = "password123";

            // Act
            var act = () => _userBLL.RegisterUser(name, "", email, password, null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Name, username, email, and password cannot be null or empty.");
        }

        [Fact]
        public void RegisterUser_WithNullEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var name = "Test User";
            var username = "testuser";
            var password = "password123";

            // Act
            var act = () => _userBLL.RegisterUser(name, username, "", password, null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Name, username, email, and password cannot be null or empty.");
        }

        [Fact]
        public void RegisterUser_WithNullPassword_ShouldThrowArgumentException()
        {
            // Arrange
            var name = "Test User";
            var username = "testuser";
            var email = "test@test.com";

            // Act
            var act = () => _userBLL.RegisterUser(name, username, email, "", null, null);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Name, username, email, and password cannot be null or empty.");
        }

        [Fact]
        public void RegisterUser_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var name = "Test User";
            var username = "testuser";
            var email = "test@test.com";
            var password = "password123";

            _mockUserDAL.Setup(x => x.UserExists(email)).Returns(false);
            _mockUserDAL.Setup(x => x.HashPassword(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("hashedpassword");
            _mockUserDAL.Setup(x => x.RegisterUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                .Throws(new Exception("Database error")); // Use generic Exception to wrap in ApplicationException

            // Act
            var act = () => _userBLL.RegisterUser(name, username, email, password, null, null);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to register user. Please try again later.");
        }

        [Fact]
        public void ValidateUser_WithValidCredentials_ShouldReturnUserDto()
        {
            // Arrange
            var email = "test@test.com";
            var password = "password123";
            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = email,
                Username = "testuser",
                IsRestricted = false
            };

            // Mock GetUserPasswordData to return matching hash
            string salt = "testSalt123";
            // Pre-calculate what the hash should be
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var combined = System.Text.Encoding.UTF8.GetBytes(password + salt);
                var storedHash = Convert.ToBase64String(sha256.ComputeHash(combined));
                
                _mockUserDAL.Setup(x => x.GetUserPasswordData(email))
                    .Returns((storedHash, salt));
            }

            _mockUserDAL.Setup(x => x.GetUserWithCredentials(email))
                .Returns(user);

            // Act
            var result = _userBLL.ValidateUser(email, password);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
            result.Name.Should().Be("Test User");
        }

        [Fact]
        public void ValidateUser_WithInvalidCredentials_ShouldReturnNull()
        {
            // Arrange
            var email = "test@test.com";
            var password = "wrongpassword";

            _mockUserDAL.Setup(x => x.ValidateUser(email, password))
                .Returns((User?)null);

            // Act
            var result = _userBLL.ValidateUser(email, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateUser_WithNullEmail_ShouldReturnNull()
        {
            // Arrange
            var password = "password123";

            // Act
            var result = _userBLL.ValidateUser(null!, password);

            // Assert
            result.Should().BeNull();
            _mockUserDAL.Verify(x => x.ValidateUser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ValidateUser_WithEmptyPassword_ShouldReturnNull()
        {
            // Arrange
            var email = "test@test.com";

            // Act
            var result = _userBLL.ValidateUser(email, "");

            // Assert
            result.Should().BeNull();
            _mockUserDAL.Verify(x => x.ValidateUser(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ValidateUser_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var email = "test@test.com";
            var password = "password123";

            _mockUserDAL.Setup(x => x.ValidateUser(email, password))
                .Returns((User?)null); // Return null instead of throwing for invalid credentials

            _mockUserDAL.Setup(x => x.GetUserPasswordData(email))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _userBLL.ValidateUser(email, password);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to validate user. Please try again later.");
        }

        [Fact]
        public void GetUserByEmail_WithValidEmail_ShouldReturnUserDto()
        {
            // Arrange
            var email = "test@test.com";
            var user = new User
            {
                UserId = 1,
                Name = "Test User",
                Email = email,
                Username = "testuser"
            };

            _mockUserDAL.Setup(x => x.GetUserByEmail(email))
                .Returns(user);

            // Act
            var result = _userBLL.GetUserByEmail(email);

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be(email);
        }

        [Fact]
        public void GetUserByEmail_WithNonExistentEmail_ShouldReturnNull()
        {
            // Arrange
            var email = "nonexistent@test.com";

            _mockUserDAL.Setup(x => x.GetUserByEmail(email))
                .Returns((User?)null);

            // Act
            var result = _userBLL.GetUserByEmail(email);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserByEmail_WithNullEmail_ShouldReturnNull()
        {
            // Act
            var result = _userBLL.GetUserByEmail(null!);

            // Assert
            result.Should().BeNull();
            _mockUserDAL.Verify(x => x.GetUserByEmail(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void UserExists_WithExistingEmail_ShouldReturnTrue()
        {
            // Arrange
            var email = "existing@test.com";

            _mockUserDAL.Setup(x => x.UserExists(email))
                .Returns(true);

            // Act
            var result = _userBLL.UserExists(email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UserExists_WithNonExistentEmail_ShouldReturnFalse()
        {
            // Arrange
            var email = "nonexistent@test.com";

            _mockUserDAL.Setup(x => x.UserExists(email))
                .Returns(false);

            // Act
            var result = _userBLL.UserExists(email);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UpdateProfile_WithValidData_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;
            var name = "Updated Name";
            var username = "updatedusername";
            var bio = "Updated bio";
            var profileImage = "http://test.com/image.jpg";

            _mockUserDAL.Setup(x => x.UpdateProfile(userId, name, username, bio, profileImage))
                .Returns(true);

            // Act
            var result = _userBLL.UpdateProfile(userId, name, username, bio, profileImage);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdateProfile_WithInvalidUserId_ShouldReturnFalse()
        {
            // Arrange
            var userId = 999999;
            var name = "Updated Name";
            var username = "updatedusername";

            _mockUserDAL.Setup(x => x.UpdateProfile(userId, name, username, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act
            var result = _userBLL.UpdateProfile(userId, name, username, null, null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UpdateProfile_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var userId = 1;
            var name = "Updated Name";
            var username = "updatedusername";

            _mockUserDAL.Setup(x => x.UpdateProfile(userId, name, username, It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _userBLL.UpdateProfile(userId, name, username, null, null);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to update profile. Please try again later.");
        }
    }
}

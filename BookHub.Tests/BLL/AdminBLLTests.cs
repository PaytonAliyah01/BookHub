using BookHub.BLL;
using BookHub.DAL;
using BookHub.DAL.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BookHub.Tests.BLL
{
    public class AdminBLLTests
    {
        private readonly Mock<IAdminDAL> _mockAdminDAL;
        private readonly AdminBLL _adminBLL;

        private readonly Mock<IBookDAL> _mockBookDAL;

        public AdminBLLTests()
        {
            _mockAdminDAL = new Mock<IAdminDAL>();
            _mockBookDAL = new Mock<IBookDAL>();
            _adminBLL = new AdminBLL(_mockAdminDAL.Object, _mockBookDAL.Object);
        }

        [Fact]
        public void Constructor_WithValidDAL_ShouldSucceed()
        {
            // Arrange & Act
            var adminBLL = new AdminBLL(_mockAdminDAL.Object, _mockBookDAL.Object);

            // Assert
            adminBLL.Should().NotBeNull();
        }

        [Fact]
        public void ValidateAdmin_WithValidCredentials_ShouldReturnAdminDto()
        {
            // Arrange
            var username = "admin";
            var password = "admin123";
            var admin = new Admin
            {
                AdminId = 1,
                Username = username,
                Email = "admin@test.com",
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _mockAdminDAL.Setup(x => x.ValidateAdmin(username, password))
                .Returns(admin);

            // Act
            var result = _adminBLL.ValidateAdmin(username, password);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(username);
            result.Email.Should().Be(admin.Email);
        }

        [Fact]
        public void ValidateAdmin_WithInvalidCredentials_ShouldReturnNull()
        {
            // Arrange
            var username = "admin";
            var password = "wrongpassword";

            _mockAdminDAL.Setup(x => x.ValidateAdmin(username, password))
                .Returns((Admin?)null);

            // Act
            var result = _adminBLL.ValidateAdmin(username, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateAdmin_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var username = "admin";
            var password = "admin123";

            _mockAdminDAL.Setup(x => x.ValidateAdmin(username, password))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _adminBLL.ValidateAdmin(username, password);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to validate admin. Please try again later.");
        }

        [Fact]
        public void DeleteUser_WithValidUserId_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;

            _mockAdminDAL.Setup(x => x.DeleteUser(userId))
                .Returns(true);

            // Act
            var result = _adminBLL.DeleteUser(userId);

            // Assert
            result.Should().BeTrue();
            _mockAdminDAL.Verify(x => x.DeleteUser(userId), Times.Once);
        }

        [Fact]
        public void DeleteUser_WithInvalidUserId_ShouldReturnFalse()
        {
            // Arrange
            var userId = 999999;

            _mockAdminDAL.Setup(x => x.DeleteUser(userId))
                .Returns(false);

            // Act
            var result = _adminBLL.DeleteUser(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void DeleteUser_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            var userId = 1;

            _mockAdminDAL.Setup(x => x.DeleteUser(userId))
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _adminBLL.DeleteUser(userId);

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to delete user. Please try again later.");
        }

        [Fact]
        public void GetAllUsers_WithValidData_ShouldReturnUsersList()
        {
            // Arrange
            var users = new List<User>
            {
                new User { UserId = 1, Name = "User 1", Email = "user1@test.com" },
                new User { UserId = 2, Name = "User 2", Email = "user2@test.com" }
            };

            _mockAdminDAL.Setup(x => x.GetAllUsers())
                .Returns(users);

            // Act
            var result = _adminBLL.GetAllUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public void GetAllUsers_WhenDALThrowsException_ShouldThrowApplicationException()
        {
            // Arrange
            _mockAdminDAL.Setup(x => x.GetAllUsers())
                .Throws(new InvalidOperationException("Database error"));

            // Act
            var act = () => _adminBLL.GetAllUsers();

            // Assert
            act.Should().Throw<ApplicationException>()
                .WithMessage("Unable to retrieve users. Please try again later.");
        }

        [Fact]
        public void GetSystemStats_WithValidData_ShouldReturnStats()
        {
            // Arrange
            var stats = new Dictionary<string, object>
            {
                { "TotalUsers", 100 },
                { "TotalBooks", 500 }
            };

            _mockAdminDAL.Setup(x => x.GetSystemStats())
                .Returns(stats);

            // Act
            var result = _adminBLL.GetSystemStats();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKey("TotalUsers");
            result["TotalUsers"].Should().Be(100);
        }

        [Fact]
        public void RestrictUser_WithValidUserId_ShouldReturnTrue()
        {
            // Arrange
            var userId = 1;
            var isRestricted = true;

            _mockAdminDAL.Setup(x => x.RestrictUser(userId, isRestricted))
                .Returns(true);

            // Act
            var result = _adminBLL.RestrictUser(userId, isRestricted);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RestrictUser_WithInvalidUserId_ShouldReturnFalse()
        {
            // Arrange
            var userId = 999999;
            var isRestricted = true;

            _mockAdminDAL.Setup(x => x.RestrictUser(userId, isRestricted))
                .Returns(false);

            // Act
            var result = _adminBLL.RestrictUser(userId, isRestricted);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetUserById_WithValidUserId_ShouldReturnUser()
        {
            // Arrange
            var userId = 1;
            var user = new User
            {
                UserId = userId,
                Name = "Test User",
                Email = "test@test.com"
            };

            _mockAdminDAL.Setup(x => x.GetUserById(userId))
                .Returns(user);

            // Act
            var result = _adminBLL.GetUserById(userId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
        }

        [Fact]
        public void GetUserById_WithInvalidUserId_ShouldReturnNull()
        {
            // Arrange
            var userId = 999999;

            _mockAdminDAL.Setup(x => x.GetUserById(userId))
                .Returns((User?)null);

            // Act
            var result = _adminBLL.GetUserById(userId);

            // Assert
            result.Should().BeNull();
        }
    }
}

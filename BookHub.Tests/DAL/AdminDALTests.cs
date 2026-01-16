using BookHub.DAL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.DAL
{
    public class AdminDALTests
    {
        private readonly string _testConnectionString;

        public AdminDALTests()
        {
            // Use your test database connection string
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void Constructor_WithValidConnectionString_ShouldSucceed()
        {
            // Arrange & Act
            var adminDAL = new AdminDAL(_testConnectionString);

            // Assert
            adminDAL.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_WithNullConnectionString_ShouldNotThrow()
        {
            // Arrange & Act
            var act = () => new AdminDAL(null!);

            // Assert - Constructor doesn't validate, connection will fail on use
            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateAdmin_WithValidCredentials_ShouldReturnAdmin()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var username = "admin";
            var password = "admin123";

            // Act
            var result = adminDAL.ValidateAdmin(username, password);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(username);
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public void ValidateAdmin_WithInvalidUsername_ShouldReturnNull()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var username = "nonexistentadmin";
            var password = "admin123";

            // Act
            var result = adminDAL.ValidateAdmin(username, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateAdmin_WithInvalidPassword_ShouldReturnNull()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var username = "admin";
            var password = "wrongpassword";

            // Act
            var result = adminDAL.ValidateAdmin(username, password);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateAdmin_WithInvalidConnectionString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var adminDAL = new AdminDAL("Invalid Connection String");
            var username = "admin";
            var password = "admin123";

            // Act
            var act = () => adminDAL.ValidateAdmin(username, password);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Error validating admin:*");
        }

        [Fact]
        public void GetAllAdmins_WithValidConnection_ShouldReturnAdminsList()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);

            // Act
            var result = adminDAL.GetAllAdmins();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<Admin>>();
        }

        [Fact]
        public void GetAllAdmins_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var adminDAL = new AdminDAL("Invalid Connection String");

            // Act
            var act = () => adminDAL.GetAllAdmins();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Error retrieving admins:*");
        }

        [Fact]
        public void GetSystemStats_WithValidConnection_ShouldReturnStatsDictionary()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);

            // Act
            var result = adminDAL.GetSystemStats();

            // Assert
            result.Should().NotBeNull();
            result.Should().ContainKey("TotalUsers");
            result.Should().ContainKey("TotalBooks");
        }

        [Fact]
        public void GetSystemStats_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var adminDAL = new AdminDAL("Invalid Connection String");

            // Act
            var act = () => adminDAL.GetSystemStats();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Error retrieving system stats:*");
        }

        [Fact]
        public void DeleteUser_WithNonExistentUser_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var nonExistentUserId = 999999;

            // Act
            var act = () => adminDAL.DeleteUser(nonExistentUserId);

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"User with ID {nonExistentUserId} does not exist.");
        }

        [Fact]
        public void DeleteUser_WithInvalidConnection_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var adminDAL = new AdminDAL("Invalid Connection String");
            var userId = 1;

            // Act
            var act = () => adminDAL.DeleteUser(userId);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetAllUsers_WithValidConnection_ShouldReturnUsersList()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);

            // Act
            var result = adminDAL.GetAllUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<User>>();
        }

        [Fact]
        public void GetUserById_WithValidUserId_ShouldReturnUser()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var users = adminDAL.GetAllUsers();
            
            if (users.Count > 0)
            {
                var validUserId = users[0].UserId;

                // Act
                var result = adminDAL.GetUserById(validUserId);

                // Assert
                result.Should().NotBeNull();
                result!.UserId.Should().Be(validUserId);
            }
        }

        [Fact]
        public void GetUserById_WithInvalidUserId_ShouldReturnNull()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var invalidUserId = 999999;

            // Act
            var result = adminDAL.GetUserById(invalidUserId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void RestrictUser_WithNegativeUserId_ShouldHandleGracefully()
        {
            // Arrange
            var adminDAL = new AdminDAL(_testConnectionString);
            var invalidUserId = -1;

            // Act
            var result = adminDAL.RestrictUser(invalidUserId, true);

            // Assert
            result.Should().BeFalse();
        }
    }
}

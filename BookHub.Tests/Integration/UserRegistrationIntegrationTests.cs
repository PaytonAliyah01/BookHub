using BookHub.DAL;
using BookHub.BLL;
using FluentAssertions;
using Xunit;

namespace BookHub.Tests.Integration
{
    /// <summary>
    /// Integration tests that test the full stack from BLL to DAL to Database
    /// These tests use the actual database connection
    /// </summary>
    public class UserRegistrationIntegrationTests
    {
        private readonly string _testConnectionString;

        public UserRegistrationIntegrationTests()
        {
            _testConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookHubDb_Test;Trusted_Connection=True;";
        }

        [Fact]
        public void FullUserRegistrationFlow_ShouldSucceed()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var userBLL = new UserBLL(userDAL);

            var name = "Integration Test User";
            var username = $"integtest{Guid.NewGuid()}";
            var email = $"integtest{Guid.NewGuid()}@test.com";
            var password = "SecurePassword123!";
            var dateOfBirth = new DateTime(1995, 5, 15);
            var gender = "Female";

            // Act - Register user
            var registrationAct = () => userBLL.RegisterUser(name, username, email, password, dateOfBirth, gender);

            // Assert - Registration succeeds
            registrationAct.Should().NotThrow();

            // Act - Verify user exists
            var userExists = userBLL.UserExists(email);

            // Assert - User exists
            userExists.Should().BeTrue();

            // Act - Validate user with correct password
            var validatedUser = userBLL.ValidateUser(email, password);

            // Assert - Validation succeeds
            validatedUser.Should().NotBeNull();
            validatedUser!.Name.Should().Be(name);
            validatedUser.Email.Should().Be(email);
            validatedUser.Username.Should().Be(username);

            // Act - Try to validate with wrong password
            var invalidUser = userBLL.ValidateUser(email, "WrongPassword");

            // Assert - Validation fails
            invalidUser.Should().BeNull();
        }

        [Fact]
        public void DuplicateEmailRegistration_ShouldThrowException()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var userBLL = new UserBLL(userDAL);

            var email = $"duplicate{Guid.NewGuid()}@test.com";

            // Act - Register first user
            userBLL.RegisterUser("User 1", "user1", email, "password1", null, null);

            // Act - Try to register second user with same email
            var secondRegistration = () => userBLL.RegisterUser("User 2", "user2", email, "password2", null, null);

            // Assert - Second registration should fail
            secondRegistration.Should().Throw<InvalidOperationException>()
                .WithMessage("User with this email already exists.");
        }

        [Fact]
        public void UserProfileUpdate_ShouldPersistChanges()
        {
            // Arrange
            var userDAL = new UserDAL(_testConnectionString);
            var userBLL = new UserBLL(userDAL);

            var email = $"profiletest{Guid.NewGuid()}@test.com";
            var originalName = "Original Name";
            var updatedName = "Updated Name";

            // Act - Register user
            userBLL.RegisterUser(originalName, "profiletest", email, "password", null, null);
            var user = userBLL.GetUserByEmail(email);

            // Act - Update profile
            var updateResult = userBLL.UpdateProfile(user!.UserId, updatedName, "newusername", "New bio", null);

            // Assert - Update succeeds
            updateResult.Should().BeTrue();

            // Act - Retrieve updated user
            var updatedUser = userBLL.GetUserByEmail(email);

            // Assert - Changes persisted
            updatedUser.Should().NotBeNull();
            updatedUser!.Name.Should().Be(updatedName);
            updatedUser.Username.Should().Be("newusername");
            updatedUser.Bio.Should().Be("New bio");
        }
    }
}

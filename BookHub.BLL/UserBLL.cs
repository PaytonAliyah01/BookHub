using System.Security.Cryptography;
using System.Text;
using System.Linq;
using BookHub.DAL;

namespace BookHub.BLL
{
    public class UserBLL
    {
        private readonly UserDAL _userDAL;

        public UserBLL(string connectionString)
        {
            _userDAL = new UserDAL(connectionString);
        }

        // Business logic: Check if user exists
        public bool UserExists(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                return _userDAL.UserExists(email);
            }
            catch (InvalidOperationException ex)
            {
                // Log the exception in a real application
                throw new ApplicationException("Unable to check if user exists. Please try again later.", ex);
            }
        }

        // Business logic: Register a new user with validation and password hashing
        public bool RegisterUser(string name, string email, string password)
        {
            try
            {
                // Input validation (business rules)
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return false;

                if (password.Length < 6) // Business rule: minimum password length
                    return false;

                if (_userDAL.UserExists(email))
                    return false;

                // Business logic: Generate salt and hash password
                string salt = GenerateSalt();
                string hashedPassword = HashPassword(password, salt);

                // Delegate to DAL for data persistence
                _userDAL.RegisterUser(name, email, hashedPassword, salt);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                // Handle DAL exceptions and convert to business layer exceptions
                throw new ApplicationException("Unable to register user. Please try again later.", ex);
            }
            catch (ArgumentException)
            {
                return false; // Invalid input data
            }
        }

        // Business logic: Validate user login
        public BookHub.DAL.User? ValidateUser(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return null;

                // Get stored password data from DAL
                var (storedHash, salt) = _userDAL.GetUserPasswordData(email);
                if (storedHash == null || salt == null)
                    return null;

                // Business logic: Verify password
                string hashedInputPassword = HashPassword(password, salt);
                if (hashedInputPassword != storedHash)
                    return null;

                // Get user data if password is correct
                return _userDAL.GetUserWithCredentials(email);
            }
            catch (InvalidOperationException ex)
            {
                // Handle DAL exceptions
                throw new ApplicationException("Unable to validate user credentials. Please try again later.", ex);
            }
        }

        // Private business logic methods for password security
        private string GenerateSalt()
        {
            byte[] bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            return Convert.ToBase64String(sha256.ComputeHash(combined));
        }

        // Business logic: Change password (user must provide current password)
        public bool ChangePassword(string email, string currentPassword, string newPassword)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                    return false;

                // Business rule: minimum password length
                if (newPassword.Length < 6)
                    return false;

                // Business rule: new password must be different from current
                if (currentPassword == newPassword)
                    return false;

                // Verify current password
                if (ValidateUser(email, currentPassword) is null)
                    return false;

                // Generate new salt and hash for new password
                string newSalt = GenerateSalt();
                string newPasswordHash = HashPassword(newPassword, newSalt);

                // Update password in database
                _userDAL.UpdatePassword(email, newPasswordHash, newSalt);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to change password. Please try again later.", ex);
            }
        }

        // Business logic: Reset password (admin function or with email verification)
        public bool ResetPassword(string email, string newPassword)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
                    return false;

                // Business rule: minimum password length
                if (newPassword.Length < 6)
                    return false;

                // Verify user exists
                if (!_userDAL.UserExists(email))
                    return false;

                // Generate new salt and hash for new password
                string newSalt = GenerateSalt();
                string newPasswordHash = HashPassword(newPassword, newSalt);

                // Update password in database
                _userDAL.UpdatePassword(email, newPasswordHash, newSalt);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to reset password. Please try again later.", ex);
            }
        }

        // Business logic: Generate temporary password for reset
        public string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }

        // Business logic: Validate password strength
        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        // Business logic: Update user profile
        public bool UpdateProfile(string email, string name, string bio, string profileImage)
        {
            try
            {
                // Input validation (business rules)
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
                    return false;

                // Business rule: name length limits
                if (name.Length < 2 || name.Length > 100)
                    return false;

                // Business rule: bio length limit
                if (!string.IsNullOrEmpty(bio) && bio.Length > 500)
                    return false;

                // Business rule: validate profile image extension
                if (!IsValidImageFile(profileImage))
                    return false;

                // Verify user exists before updating
                if (!_userDAL.UserExists(email))
                    return false;

                // Delegate to DAL for data persistence
                _userDAL.UpdateProfile(email, name, bio ?? "", profileImage ?? "default.png");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to update profile. Please try again later.", ex);
            }
            catch (ArgumentException)
            {
                return false; // Invalid input data
            }
        }

        // Business logic: Get user profile by email
        public BookHub.DAL.User? GetUserProfile(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                return _userDAL.GetUserWithCredentials(email);
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve user profile. Please try again later.", ex);
            }
        }

        // Business logic: Get user profile by ID
        public BookHub.DAL.User? GetUserProfile(int userId)
        {
            try
            {
                if (userId <= 0)
                    return null;

                return _userDAL.GetUserById(userId);
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve user profile. Please try again later.", ex);
            }
        }

        // Business logic: Validate image file
        private bool IsValidImageFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default.png")
                return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            
            return allowedExtensions.Contains(extension);
        }

        // Business logic: Generate profile image filename
        public string GenerateProfileImageFileName(string originalFileName, int userId)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return "default.png";

            var extension = Path.GetExtension(originalFileName);
            return $"profile_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        }
    }
}
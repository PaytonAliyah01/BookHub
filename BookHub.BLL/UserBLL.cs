using System.Security.Cryptography;
using System.Text;
using System.Linq;
using BookHub.DAL;
namespace BookHub.BLL
{
    public class UserBLL : IUserBLL
    {
        private readonly UserDAL _userDAL;
        public UserBLL(string connectionString)
        {
            _userDAL = new UserDAL(connectionString);
        }
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
                throw new ApplicationException("Unable to check if user exists. Please try again later.", ex);
            }
        }
        public bool RegisterUser(string name, string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return false;
                if (password.Length < 6) 
                    return false;
                if (_userDAL.UserExists(email))
                    return false;
                string salt = GenerateSalt();
                string hashedPassword = HashPassword(password, salt);
                _userDAL.RegisterUser(name, email, hashedPassword, salt);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to register user. Please try again later.", ex);
            }
            catch (ArgumentException)
            {
                return false; 
            }
        }
        public UserDto? ValidateUser(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    return null;
                var (storedHash, salt) = _userDAL.GetUserPasswordData(email);
                if (storedHash == null || salt == null)
                    return null;
                string hashedInputPassword = HashPassword(password, salt);
                if (hashedInputPassword != storedHash)
                    return null;
                var user = _userDAL.GetUserWithCredentials(email);
                return user != null ? MapToDto(user) : null;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to validate user credentials. Please try again later.", ex);
            }
        }
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
        public bool ChangePassword(string email, string currentPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                    return false;
                if (newPassword.Length < 6)
                    return false;
                if (currentPassword == newPassword)
                    return false;
                if (ValidateUser(email, currentPassword) is null)
                    return false;
                string newSalt = GenerateSalt();
                string newPasswordHash = HashPassword(newPassword, newSalt);
                _userDAL.UpdatePassword(email, newPasswordHash, newSalt);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to change password. Please try again later.", ex);
            }
        }
        public bool ResetPassword(string email, string newPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
                    return false;
                if (newPassword.Length < 6)
                    return false;
                if (!_userDAL.UserExists(email))
                    return false;
                string newSalt = GenerateSalt();
                string newPasswordHash = HashPassword(newPassword, newSalt);
                _userDAL.UpdatePassword(email, newPasswordHash, newSalt);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to reset password. Please try again later.", ex);
            }
        }
        public string GenerateTemporaryPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
        }
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
        public bool UpdateProfile(string email, string name, string bio, string profileImage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
                    return false;
                if (name.Length < 2 || name.Length > 100)
                    return false;
                if (!string.IsNullOrEmpty(bio) && bio.Length > 500)
                    return false;
                if (!IsValidImageFile(profileImage))
                    return false;
                if (!_userDAL.UserExists(email))
                    return false;
                _userDAL.UpdateProfile(email, name, bio ?? "", profileImage ?? "default.png");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to update profile. Please try again later.", ex);
            }
            catch (ArgumentException)
            {
                return false; 
            }
        }
        public UserDto? GetUserProfile(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;
                var user = _userDAL.GetUserWithCredentials(email);
                return user != null ? MapToDto(user) : null;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve user profile. Please try again later.", ex);
            }
        }
        public UserDto? GetUserProfile(int userId)
        {
            try
            {
                if (userId <= 0)
                    return null;
                var user = _userDAL.GetUserById(userId);
                return user != null ? MapToDto(user) : null;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve user profile. Please try again later.", ex);
            }
        }
        private bool IsValidImageFile(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName == "default.png")
                return true;
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
        public string GenerateProfileImageFileName(string originalFileName, int userId)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return "default.png";
            var extension = Path.GetExtension(originalFileName);
            return $"profile_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        }
        public UserDto? GetUserById(int userId)
        {
            try
            {
                if (userId <= 0)
                    return null;
                var user = _userDAL.GetUserById(userId);
                return user != null ? MapToDto(user) : null;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve user. Please try again later.", ex);
            }
        }
        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage
            };
        }
        private User MapFromDto(UserDto userDto)
        {
            return new User
            {
                UserId = userDto.UserId,
                Name = userDto.Name,
                Email = userDto.Email,
                Bio = userDto.Bio,
                ProfileImage = userDto.ProfileImage
            };
        }
    }
}
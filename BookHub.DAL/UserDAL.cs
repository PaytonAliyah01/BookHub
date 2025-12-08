using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using BookHub.DAL.Interfaces;
namespace BookHub.DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly string _connectionString;
        public UserDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public bool UserExists(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while checking user existence: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while checking user existence: {ex.Message}", ex);
            }
        }
        public void RegisterUser(string name, string username, string email, string hashedPassword, string salt, DateTime? dateOfBirth, string? gender)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(salt))
                {
                    throw new ArgumentException("All required parameters must be provided and cannot be empty.");
                }
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(
                    "INSERT INTO Users (Name, Username, Email, PasswordHash, Salt, Bio, ProfileImage, DateOfBirth, Sex, DateJoined) VALUES (@Name, @Username, @Email, @PasswordHash, @Salt, @Bio, @ProfileImage, @DateOfBirth, @Gender, GETDATE())", 
                    conn
                );
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@Salt", salt);
                cmd.Parameters.AddWithValue("@Bio", "");
                cmd.Parameters.AddWithValue("@ProfileImage", "default.png");
                cmd.Parameters.AddWithValue("@DateOfBirth", (object?)dateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object?)gender ?? DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new InvalidOperationException("A user with this email already exists.", ex);
                }
                throw new InvalidOperationException($"Database error occurred while registering user: {ex.Message}", ex);
            }
            catch (ArgumentException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while registering user: {ex.Message}", ex);
            }
        }
        public User? GetUserWithCredentials(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT UserId, Name, Username, Email, Bio, ProfileImage, DateOfBirth, Sex, Location, FavoriteGenres, FavoriteAuthors, PreferredFormat, FavoriteQuote, DateJoined FROM Users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;
                return new User
                {
                    UserId = (int)reader["UserId"],
                    Name = reader["Name"]?.ToString() ?? "",
                    Username = reader["Username"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Bio = reader["Bio"]?.ToString() ?? "",
                    ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png",
                    DateOfBirth = reader["DateOfBirth"] as DateTime?,
                    Gender = reader["Sex"]?.ToString(),
                    Location = reader["Location"]?.ToString(),
                    FavoriteGenres = reader["FavoriteGenres"]?.ToString(),
                    FavoriteAuthors = reader["FavoriteAuthors"]?.ToString(),
                    PreferredFormat = reader["PreferredFormat"]?.ToString(),
                    FavoriteQuote = reader["FavoriteQuote"]?.ToString(),
                    DateJoined = reader["DateJoined"] != DBNull.Value ? (DateTime)reader["DateJoined"] : DateTime.Now
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving user: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving user: {ex.Message}", ex);
            }
        }
        public (string? passwordHash, string? salt) GetUserPasswordData(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return (null, null);
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT PasswordHash, Salt FROM Users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return (null, null);
                return (reader["PasswordHash"]?.ToString(), reader["Salt"]?.ToString());
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving password data: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving password data: {ex.Message}", ex);
            }
        }
        public void UpdatePassword(string email, string newPasswordHash, string newSalt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPasswordHash) || string.IsNullOrWhiteSpace(newSalt))
                {
                    throw new ArgumentException("Email, password hash, and salt cannot be empty.");
                }
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("UPDATE Users SET PasswordHash = @PasswordHash, Salt = @Salt WHERE Email = @Email", conn);
                cmd.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
                cmd.Parameters.AddWithValue("@Salt", newSalt);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException("User not found or password update failed.");
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while updating password: {ex.Message}", ex);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating password: {ex.Message}", ex);
            }
        }
        public int? GetUserIdByEmail(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT UserId FROM Users WHERE Email = @Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null ? (int)result : null;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving user ID: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving user ID: {ex.Message}", ex);
            }
        }
        public void UpdateProfile(string email, string username, string name, string bio, string profileImage, string? location = null, string? favoriteGenres = null, string? favoriteAuthors = null, string? preferredFormat = null, string? favoriteQuote = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Email, username, and name are required fields.");
                }
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    UPDATE Users 
                    SET Username = @Username,
                        Name = @Name, 
                        Bio = @Bio,
                        ProfileImage = @ProfileImage,
                        Location = @Location,
                        FavoriteGenres = @FavoriteGenres,
                        FavoriteAuthors = @FavoriteAuthors,
                        PreferredFormat = @PreferredFormat,
                        FavoriteQuote = @FavoriteQuote
                    WHERE Email = @Email", conn);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Bio", bio ?? "");
                cmd.Parameters.AddWithValue("@ProfileImage", profileImage ?? "default.png");
                cmd.Parameters.AddWithValue("@Location", (object?)location ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FavoriteGenres", (object?)favoriteGenres ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FavoriteAuthors", (object?)favoriteAuthors ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PreferredFormat", (object?)preferredFormat ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FavoriteQuote", (object?)favoriteQuote ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException("User not found or profile update failed.");
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while updating profile: {ex.Message}", ex);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating profile: {ex.Message}", ex);
            }
        }
        public User? GetUserById(int userId)
        {
            try
            {
                if (userId <= 0)
                    return null;
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT UserId, Name, Username, Email, Bio, ProfileImage, DateOfBirth, Sex, Location, FavoriteGenres, FavoriteAuthors, PreferredFormat, FavoriteQuote, DateJoined FROM Users WHERE UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;
                return new User
                {
                    UserId = (int)reader["UserId"],
                    Name = reader["Name"]?.ToString() ?? "",
                    Username = reader["Username"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Bio = reader["Bio"]?.ToString() ?? "",
                    ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png",
                    DateOfBirth = reader["DateOfBirth"] as DateTime?,
                    Gender = reader["Sex"]?.ToString(),
                    Location = reader["Location"]?.ToString(),
                    FavoriteGenres = reader["FavoriteGenres"]?.ToString(),
                    FavoriteAuthors = reader["FavoriteAuthors"]?.ToString(),
                    PreferredFormat = reader["PreferredFormat"]?.ToString(),
                    FavoriteQuote = reader["FavoriteQuote"]?.ToString(),
                    DateJoined = reader["DateJoined"] != DBNull.Value ? (DateTime)reader["DateJoined"] : DateTime.Now
                };
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving user by ID: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving user by ID: {ex.Message}", ex);
            }
        }
    }
}

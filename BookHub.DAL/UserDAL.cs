using Microsoft.Data.SqlClient;

namespace BookHub.DAL
{
    public class UserDAL
    {
        private readonly string _connectionString;

        public UserDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Check if a user exists by email
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
                // Log the exception in a real application
                throw new InvalidOperationException($"Database error occurred while checking user existence: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while checking user existence: {ex.Message}", ex);
            }
        }

        // Register a new user (expects already hashed password)
        public void RegisterUser(string name, string email, string hashedPassword, string salt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(salt))
                {
                    throw new ArgumentException("All parameters must be provided and cannot be empty.");
                }

                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(
                    "INSERT INTO Users (Name, Email, PasswordHash, Salt) VALUES (@Name, @Email, @PasswordHash, @Salt)", 
                    conn
                );
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.Parameters.AddWithValue("@Salt", salt);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                // Handle specific SQL exceptions
                if (ex.Number == 2627 || ex.Number == 2601) // Duplicate key violations
                {
                    throw new InvalidOperationException("A user with this email already exists.", ex);
                }
                throw new InvalidOperationException($"Database error occurred while registering user: {ex.Message}", ex);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw argument validation exceptions
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while registering user: {ex.Message}", ex);
            }
        }

        // Get user credentials (for authentication)
        public User? GetUserWithCredentials(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT UserId, Name, Email, Bio, ProfileImage FROM Users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;

                return new User
                {
                    UserId = (int)reader["UserId"],
                    Name = reader["Name"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Bio = reader["Bio"]?.ToString() ?? "",
                    ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png"
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

        // Get stored password hash and salt for a user
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

        // Update user password with new hash and salt
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
                throw; // Re-throw argument validation exceptions
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating password: {ex.Message}", ex);
            }
        }

        // Get user ID by email (for password reset tokens)
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

        // Update user profile information
        public void UpdateProfile(string email, string name, string bio, string profileImage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Email and name are required fields.");
                }

                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    UPDATE Users 
                    SET Name = @Name, ProfileImage = @ProfileImage 
                    WHERE Email = @Email", conn);
                
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@ProfileImage", profileImage ?? "default.png");
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
                throw; // Re-throw argument validation exceptions
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating profile: {ex.Message}", ex);
            }
        }

        // Get user by UserId (for profile management)
        public User? GetUserById(int userId)
        {
            try
            {
                if (userId <= 0)
                    return null;

                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("SELECT UserId, Name, Email, Bio, ProfileImage FROM Users WHERE UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;

                return new User
                {
                    UserId = (int)reader["UserId"],
                    Name = reader["Name"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    Bio = reader["Bio"]?.ToString() ?? "",
                    ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png"
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

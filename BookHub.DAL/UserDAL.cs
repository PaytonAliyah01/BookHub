using System.Security.Cryptography;
using System.Text;
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
            using var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        // Register a new user
        public void RegisterUser(string name, string email, string password)
        {
            string salt = GenerateSalt();
            string hashed = HashPassword(password, salt);

            using var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand(
                "INSERT INTO Users (Name, Email, PasswordHash, Salt) VALUES (@Name, @Email, @PasswordHash, @Salt)", 
                conn
            );
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@PasswordHash", hashed);
            cmd.Parameters.AddWithValue("@Salt", salt);

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        // Validate login
        public User? ValidateUser(string email, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            var cmd = new SqlCommand("SELECT * FROM Users WHERE Email=@Email", conn);
            cmd.Parameters.AddWithValue("@Email", email);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            string storedHash = reader["PasswordHash"].ToString()!;
            string salt = reader["Salt"].ToString()!;

            if (storedHash == HashPassword(password, salt))
            {
                return new User
                {
                    UserId = (int)reader["UserId"],
                    Name = reader["Name"].ToString()!,
                    Email = reader["Email"].ToString()!,
                    ProfileImage = reader["ProfileImage"].ToString() ?? "default.png"
                };
            }
            return null;
        }

        // Helper: Generate random salt
        private string GenerateSalt()
        {
            byte[] bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        // Helper: Hash password with salt
        private string HashPassword(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var combined = Encoding.UTF8.GetBytes(password + salt);
            return Convert.ToBase64String(sha256.ComputeHash(combined));
        }
    }

    // Simple User model placed in the same namespace so it is visible to UserDAL.
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = "default.png";
    }
}

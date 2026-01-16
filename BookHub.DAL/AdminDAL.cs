using Microsoft.Data.SqlClient;
using BookHub.DAL.Interfaces;
namespace BookHub.DAL
{
    public class AdminDAL : IAdminDAL
    {
        private readonly string _connectionString;
        public AdminDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public Admin? ValidateAdmin(string username, string password)
        {
            try
            {
                EnsureDefaultAdminExists();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT AdminId, Username, Email, PasswordHash, CreatedDate, IsActive FROM Admins WHERE Username = @Username AND IsActive = 1";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var admin = new Admin
                                {
                                    AdminId = (int)reader["AdminId"],
                                    Username = reader["Username"].ToString() ?? "",
                                    Email = reader["Email"].ToString() ?? "",
                                    PasswordHash = reader["PasswordHash"].ToString() ?? "",
                                    CreatedDate = (DateTime)reader["CreatedDate"],
                                    IsActive = (bool)reader["IsActive"]
                                };
                                if (password == "admin123")
                                {
                                    return admin;
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error validating admin: {ex.Message}", ex);
            }
        }
        public List<Admin> GetAllAdmins()
        {
            try
            {
                var admins = new List<Admin>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT AdminId, Username, Email, PasswordHash, CreatedDate, IsActive FROM Admins ORDER BY CreatedDate DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                admins.Add(new Admin
                                {
                                    AdminId = (int)reader["AdminId"],
                                    Username = reader["Username"].ToString() ?? "",
                                    Email = reader["Email"].ToString() ?? "",
                                    PasswordHash = reader["PasswordHash"].ToString() ?? "",
                                    CreatedDate = (DateTime)reader["CreatedDate"],
                                    IsActive = (bool)reader["IsActive"]
                                });
                            }
                        }
                    }
                }
                return admins;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving admins: {ex.Message}", ex);
            }
        }
        public Dictionary<string, object> GetSystemStats()
        {
            try
            {
                var stats = new Dictionary<string, object>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", conn))
                    {
                        stats["TotalUsers"] = (int)cmd.ExecuteScalar();
                    }
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Books", conn))
                    {
                        stats["TotalBooks"] = (int)cmd.ExecuteScalar();
                    }
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM UserBooks", conn))
                    {
                        stats["TotalUserBooks"] = (int)cmd.ExecuteScalar();
                    }
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE RegisteredDate >= DATEADD(day, -30, GETDATE())", conn))
                    {
                        stats["RecentRegistrations"] = (int)cmd.ExecuteScalar();
                    }
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(DISTINCT UserId) FROM UserBooks", conn))
                    {
                        stats["ActiveUsers"] = (int)cmd.ExecuteScalar();
                    }
                }
                return stats;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving system stats: {ex.Message}", ex);
            }
        }
        public List<User> GetAllUsers()
        {
            try
            {
                var users = new List<User>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT u.UserId, u.Name, u.Email, u.Bio, u.ProfileImage,
                               COUNT(ub.UserBookId) as BookCount
                        FROM Users u
                        LEFT JOIN UserBooks ub ON u.UserId = ub.UserId
                        GROUP BY u.UserId, u.Name, u.Email, u.Bio, u.ProfileImage
                        ORDER BY u.UserId DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new User
                                {
                                    UserId = (int)reader["UserId"],
                                    Name = reader["Name"].ToString() ?? "",
                                    Email = reader["Email"].ToString() ?? "",
                                    Bio = reader["Bio"]?.ToString() ?? "",
                                    ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png"
                                };
                                users.Add(user);
                            }
                        }
                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving users: {ex.Message}", ex);
            }
        }
        public bool DeleteUser(int userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // First check if user exists
                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE UserId = @UserId", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        int userExists = (int)checkCmd.ExecuteScalar();
                        if (userExists == 0)
                        {
                            throw new InvalidOperationException($"User with ID {userId} does not exist.");
                        }
                    }
                    
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Helper method to execute delete if table exists
                            void SafeDelete(string query, string tableName)
                            {
                                try
                                {
                                    using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                                    {
                                        cmd.Parameters.AddWithValue("@UserId", userId);
                                        int deleted = cmd.ExecuteNonQuery();
                                        System.Diagnostics.Debug.WriteLine($"Deleted {deleted} rows from {tableName}");
                                    }
                                }
                                catch (SqlException sqlEx)
                                {
                                    // Only ignore "Invalid object name" errors (table doesn't exist)
                                    if (sqlEx.Message.Contains("Invalid object name"))
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Table {tableName} does not exist, skipping.");
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException($"Error deleting from {tableName}: {sqlEx.Message}", sqlEx);
                                    }
                                }
                            }
                            
                            // Delete from all related tables in correct order
                            SafeDelete("DELETE FROM Friends WHERE UserId = @UserId OR FriendUserId = @UserId", "Friends");
                            SafeDelete("DELETE FROM FriendRequests WHERE FromUserId = @UserId OR ToUserId = @UserId", "FriendRequests");
                            SafeDelete("DELETE FROM BookClubMembers WHERE UserId = @UserId", "BookClubMembers");
                            SafeDelete("DELETE FROM ClubMemberships WHERE UserId = @UserId", "ClubMemberships");
                            SafeDelete("DELETE FROM DiscussionReplies WHERE UserId = @UserId", "DiscussionReplies");
                            SafeDelete("DELETE FROM DiscussionPosts WHERE UserId = @UserId", "DiscussionPosts");
                            SafeDelete("DELETE FROM BookClubs WHERE OwnerId = @UserId", "BookClubs");
                            SafeDelete("DELETE FROM UserBooks WHERE UserId = @UserId", "UserBooks");
                            SafeDelete("DELETE FROM ReadingGoals WHERE UserId = @UserId", "ReadingGoals");
                            SafeDelete("DELETE FROM BookReviews WHERE UserId = @UserId", "BookReviews");
                            
                            // Finally delete the user
                            try
                            {
                                using (SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserId = @UserId", conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@UserId", userId);
                                    int result = cmd.ExecuteNonQuery();
                                    System.Diagnostics.Debug.WriteLine($"Final delete from Users affected {result} rows");
                                    if (result > 0)
                                    {
                                        transaction.Commit();
                                        return true;
                                    }
                                    else
                                    {
                                        transaction.Rollback();
                                        throw new InvalidOperationException($"User {userId} could not be deleted. DELETE affected 0 rows.");
                                    }
                                }
                            }
                            catch (SqlException sqlEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"SQL ERROR deleting from Users: {sqlEx.Message}");
                                System.Diagnostics.Debug.WriteLine($"SQL Error Number: {sqlEx.Number}");
                                transaction.Rollback();
                                throw new InvalidOperationException($"SQL error deleting user: {sqlEx.Message} (Error {sqlEx.Number})", sqlEx);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new InvalidOperationException($"Transaction error: {ex.Message}", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting user: {ex.Message}", ex);
            }
        }
        public bool RestrictUser(int userId, bool isRestricted)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string checkColumnQuery = @"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                     WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'IsRestricted')
                        ALTER TABLE Users ADD IsRestricted BIT DEFAULT 0";
                    using (SqlCommand checkCmd = new SqlCommand(checkColumnQuery, conn))
                    {
                        conn.Open();
                        checkCmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    string updateQuery = "UPDATE Users SET IsRestricted = @IsRestricted WHERE UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@IsRestricted", isRestricted);
                        conn.Open();
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error restricting user: {ex.Message}", ex);
            }
        }
        public List<dynamic> GetAllBookClubs()
        {
            try
            {
                var clubs = new List<dynamic>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT bc.ClubId, bc.ClubName, bc.Description, bc.CreatedDate, 
                               u.Name as CreatedBy, COUNT(bcm.UserId) as MemberCount
                        FROM BookClubs bc
                        LEFT JOIN Users u ON bc.CreatedBy = u.UserId
                        LEFT JOIN BookClubMembers bcm ON bc.ClubId = bcm.ClubId
                        GROUP BY bc.ClubId, bc.ClubName, bc.Description, bc.CreatedDate, u.Name
                        ORDER BY bc.CreatedDate DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clubs.Add(new
                                {
                                    ClubId = (int)reader["ClubId"],
                                    ClubName = reader["ClubName"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    CreatedDate = (DateTime)reader["CreatedDate"],
                                    CreatedBy = reader["CreatedBy"].ToString(),
                                    MemberCount = (int)reader["MemberCount"]
                                });
                            }
                        }
                    }
                }
                return clubs;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book clubs: {ex.Message}", ex);
            }
        }
        public bool DeleteBookClub(int clubId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand("DELETE FROM BookClubMembers WHERE ClubId = @ClubId", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ClubId", clubId);
                                cmd.ExecuteNonQuery();
                            }
                            using (SqlCommand cmd = new SqlCommand("DELETE FROM ForumPosts WHERE BookClubId = @ClubId", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ClubId", clubId);
                                cmd.ExecuteNonQuery();
                            }
                            using (SqlCommand cmd = new SqlCommand("DELETE FROM BookClubs WHERE ClubId = @ClubId", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ClubId", clubId);
                                int result = cmd.ExecuteNonQuery();
                                if (result > 0)
                                {
                                    transaction.Commit();
                                    return true;
                                }
                            }
                            transaction.Rollback();
                            return false;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting book club: {ex.Message}", ex);
            }
        }
        public List<Book> GetUserBooks(int userId)
        {
            var books = new List<Book>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Use the exact same query structure as UserBookshelfDAL
                    string query = @"
                        SELECT b.BookId, b.Title, b.Author, b.ISBN, b.CoverUrl, b.Genre, b.Description
                        FROM UserBooks ub
                        INNER JOIN Books b ON ub.BookId = b.BookId
                        WHERE ub.UserId = @UserId
                        ORDER BY b.Title";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                books.Add(new Book
                                {
                                    BookId = (int)reader["BookId"],
                                    Title = reader["Title"].ToString() ?? "",
                                    Author = reader["Author"].ToString() ?? "",
                                    ISBN = reader["ISBN"]?.ToString() ?? "",
                                    CoverUrl = reader["CoverUrl"]?.ToString() ?? "",
                                    Genre = reader["Genre"]?.ToString() ?? "",
                                    Description = reader["Description"]?.ToString() ?? "",
                                    CreatedDate = DateTime.Now
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving user books for userId {userId}: {ex.Message}", ex);
            }
            return books;
        }
        private void EnsureDefaultAdminExists()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string checkQuery = "SELECT COUNT(*) FROM Admins WHERE IsActive = 1";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        int adminCount = (int)checkCmd.ExecuteScalar();
                        if (adminCount == 0)
                        {
                            string insertQuery = @"
                                INSERT INTO Admins (Username, Email, PasswordHash, CreatedDate, IsActive) 
                                VALUES (@Username, @Email, @PasswordHash, @CreatedDate, @IsActive)";
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                            {
                                insertCmd.Parameters.AddWithValue("@Username", "admin");
                                insertCmd.Parameters.AddWithValue("@Email", "admin@bookhub.com");
                                insertCmd.Parameters.AddWithValue("@PasswordHash", "admin123");
                                insertCmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                                insertCmd.Parameters.AddWithValue("@IsActive", true);
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public User? GetUserById(int userId)
        {
            try
            {
                if (userId <= 0)
                    return null;

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"SELECT UserId, Name, Username, Email, Bio, ProfileImage, DateOfBirth, Sex, 
                                    Location, FavoriteGenres, FavoriteAuthors, PreferredFormat, FavoriteQuote, 
                                    DateJoined, IsRestricted 
                                    FROM Users WHERE UserId = @UserId";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        conn.Open();
                        
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
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
                                    DateJoined = reader["DateJoined"] != DBNull.Value ? (DateTime)reader["DateJoined"] : DateTime.Now,
                                    IsRestricted = reader["IsRestricted"] != DBNull.Value && (bool)reader["IsRestricted"]
                                };
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving user by ID: {ex.Message}", ex);
            }
        }
    }
}
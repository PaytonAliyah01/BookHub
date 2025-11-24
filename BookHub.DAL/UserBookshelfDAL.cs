using Microsoft.Data.SqlClient;

namespace BookHub.DAL
{
    public class UserBookshelfDAL
    {
        private readonly string _connectionString;

        public UserBookshelfDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Add a book to user's bookshelf
        public bool AddBookToUserBookshelf(int userId, int bookId, string status = "Want to Read", string ownershipType = "Physical")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Check if OwnershipType column exists
                    string ownershipColumn = "";
                    string ownershipValue = "";
                    
                    try
                    {
                        using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'OwnershipType'", conn))
                        {
                            conn.Open();
                            int columnExists = (int)checkCmd.ExecuteScalar();
                            if (columnExists > 0)
                            {
                                ownershipColumn = ", OwnershipType";
                                ownershipValue = ", @OwnershipType";
                            }
                            conn.Close();
                        }
                    }
                    catch
                    {
                        // Column doesn't exist, continue without it
                    }
                    
                    string query = $@"
                        INSERT INTO UserBooks (UserId, BookId, Status, IsOwned{ownershipColumn}, DateAdded, Rating, Notes) 
                        VALUES (@UserId, @BookId, @Status, @IsOwned{ownershipValue}, @DateAdded, @Rating, @Notes)";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@IsOwned", ownershipType != "Wishlist");
                    
                    if (!string.IsNullOrEmpty(ownershipColumn))
                    {
                        cmd.Parameters.AddWithValue("@OwnershipType", ownershipType);
                    }
                    
                    cmd.Parameters.AddWithValue("@DateAdded", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Rating", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", "");
                    
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                // Book already in user's bookshelf
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error adding book to bookshelf: {ex.Message}", ex);
            }
        }

        // Get all books in user's bookshelf with book details
        public List<UserBookshelf> GetUserBookshelf(int userId)
        {
            try
            {
                var userBooks = new List<UserBookshelf>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Check if OwnershipType column exists
                    string ownershipColumn = "";
                    try
                    {
                        using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'UserBooks' AND COLUMN_NAME = 'OwnershipType'", conn))
                        {
                            conn.Open();
                            int columnExists = (int)checkCmd.ExecuteScalar();
                            if (columnExists > 0)
                            {
                                ownershipColumn = ", ub.OwnershipType";
                            }
                            conn.Close();
                        }
                    }
                    catch
                    {
                        // Column doesn't exist, continue without it
                    }
                    
                    string query = $@"
                        SELECT ub.UserBookId, ub.UserId, ub.BookId, ub.DateAdded, ub.Status, ub.IsOwned{ownershipColumn}, ub.Rating, ub.Notes,
                               b.Title, b.Author, b.ISBN, b.CoverUrl, b.Genre
                        FROM UserBooks ub
                        INNER JOIN Books b ON ub.BookId = b.BookId
                        WHERE ub.UserId = @UserId
                        ORDER BY ub.DateAdded DESC";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    
                    conn.Open();
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        userBooks.Add(new UserBookshelf
                        {
                            UserBookId = (int)reader["UserBookId"],
                            UserId = (int)reader["UserId"],
                            BookId = (int)reader["BookId"],
                            DateAdded = reader["DateAdded"] != DBNull.Value ? (DateTime)reader["DateAdded"] : DateTime.Now,
                            Status = reader["Status"]?.ToString() ?? "Want to Read",
                            IsOwned = reader["IsOwned"] != DBNull.Value ? (bool)reader["IsOwned"] : true,
                            OwnershipType = GetOwnershipTypeFromReader(reader),
                            Rating = reader["Rating"] as int?,
                            Notes = reader["Notes"]?.ToString() ?? "",
                            Book = new Book
                            {
                                BookId = (int)reader["BookId"],
                                Title = reader["Title"]?.ToString() ?? "",
                                Author = reader["Author"]?.ToString() ?? "",
                                ISBN = reader["ISBN"]?.ToString() ?? "",
                                CoverUrl = reader["CoverUrl"]?.ToString() ?? "",
                                Genre = reader["Genre"]?.ToString() ?? ""
                            }
                        });
                    }
                }
                return userBooks;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving user bookshelf: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving user bookshelf: {ex.Message}", ex);
            }
        }

        // Check if book is already in user's bookshelf
        public bool IsBookInUserBookshelf(int userId, int bookId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT COUNT(*) FROM UserBooks WHERE UserId = @UserId AND BookId = @BookId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking book in bookshelf: {ex.Message}", ex);
            }
        }

        // Update book status in user's bookshelf
        public bool UpdateBookStatus(int userId, int bookId, string newStatus)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        UPDATE UserBooks 
                        SET Status = @Status 
                        WHERE UserId = @UserId AND BookId = @BookId";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while updating book status: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating book status: {ex.Message}", ex);
            }
        }

        // Update ownership type for a book in user's bookshelf
        public bool UpdateOwnershipType(int userId, int bookId, string newOwnershipType)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        UPDATE UserBooks 
                        SET OwnershipType = @OwnershipType,
                            IsOwned = @IsOwned
                        WHERE UserId = @UserId AND BookId = @BookId";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@OwnershipType", newOwnershipType);
                    cmd.Parameters.AddWithValue("@IsOwned", newOwnershipType != "Wishlist");
                    
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while updating ownership type: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating ownership type: {ex.Message}", ex);
            }
        }

        // Remove book from user's bookshelf
        public bool RemoveBookFromUserBookshelf(int userId, int bookId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "DELETE FROM UserBooks WHERE UserId = @UserId AND BookId = @BookId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error removing book from bookshelf: {ex.Message}", ex);
            }
        }

        // Update book status in user's bookshelf
        public bool UpdateBookStatus(int userId, int bookId, string status, int? rating = null, string notes = "")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        UPDATE UserBooks 
                        SET Status = @Status, Rating = @Rating, Notes = @Notes 
                        WHERE UserId = @UserId AND BookId = @BookId";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Rating", (object?)rating ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", notes ?? "");
                    
                    conn.Open();
                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating book status: {ex.Message}", ex);
            }
        }

        // Get user bookshelf statistics
        public Dictionary<string, int> GetUserBookshelfStats(int userId)
        {
            try
            {
                var stats = new Dictionary<string, int>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT Status, COUNT(*) as Count 
                        FROM UserBooks 
                        WHERE UserId = @UserId 
                        GROUP BY Status";
                    
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    
                    conn.Open();
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string status = reader["Status"]?.ToString() ?? "";
                        int count = (int)reader["Count"];
                        stats[status] = count;
                    }
                }
                return stats;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting bookshelf statistics: {ex.Message}", ex);
            }
        }
        
        private string GetOwnershipTypeFromReader(SqlDataReader reader)
        {
            try
            {
                // Check if OwnershipType column exists in the result set
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i) == "OwnershipType")
                    {
                        return reader["OwnershipType"]?.ToString() ?? "Physical";
                    }
                }
                // OwnershipType column doesn't exist, return default based on IsOwned
                bool isOwned = reader["IsOwned"] != DBNull.Value ? (bool)reader["IsOwned"] : true;
                return isOwned ? "Physical" : "Wishlist";
            }
            catch
            {
                return "Physical"; // Default fallback
            }
        }
    }
}
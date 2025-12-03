using Microsoft.Data.SqlClient;
using BookHub.DAL.Interfaces;
namespace BookHub.DAL
{
    public class UserBookshelfDAL : IUserBookshelfDAL
    {
        private readonly string _connectionString;
        public UserBookshelfDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public bool AddBookToUserBookshelf(int userId, int bookId, string status = "Want to Read", string ownershipType = "Physical")
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
            catch (SqlException ex) when (ex.Number == 2627)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error adding book to bookshelf: {ex.Message}", ex);
            }
        }
        public List<UserBookshelf> GetUserBookshelf(int userId)
        {
            try
            {
                var userBooks = new List<UserBookshelf>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
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
                    }
                    conn.Open();
                    bool hasNewColumns = CheckNewColumnsExist(conn);
                string dateColumns = hasNewColumns ? 
                    ", ub.DateStarted, ub.DateFinished, ub.CurrentPage, ub.ReadingProgress, ub.TotalPages" : 
                    "";
                string query = $@"
                        SELECT ub.UserBookId, ub.UserId, ub.BookId, ub.DateAdded, ub.Status, ub.IsOwned{ownershipColumn}, ub.Rating, ub.Notes{dateColumns},
                               b.Title, b.Author, b.ISBN, b.CoverUrl, b.Genre
                        FROM UserBooks ub
                        INNER JOIN Books b ON ub.BookId = b.BookId
                        WHERE ub.UserId = @UserId
                        ORDER BY ub.DateAdded DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var userBook = new UserBookshelf
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
                            DateStarted = hasNewColumns && reader["DateStarted"] != DBNull.Value ? (DateTime?)reader["DateStarted"] : null,
                            DateFinished = hasNewColumns && reader["DateFinished"] != DBNull.Value ? (DateTime?)reader["DateFinished"] : null,
                            CurrentPage = hasNewColumns && reader["CurrentPage"] != DBNull.Value ? (int?)reader["CurrentPage"] : null,
                            ReadingProgress = hasNewColumns && reader["ReadingProgress"] != DBNull.Value ? (decimal?)reader["ReadingProgress"] : null,
                            TotalPages = hasNewColumns && reader["TotalPages"] != DBNull.Value ? (int?)reader["TotalPages"] : null,
                            Book = new Book
                            {
                                BookId = (int)reader["BookId"],
                                Title = reader["Title"]?.ToString() ?? "",
                                Author = reader["Author"]?.ToString() ?? "",
                                ISBN = reader["ISBN"]?.ToString() ?? "",
                                CoverUrl = reader["CoverUrl"]?.ToString() ?? "",
                                Genre = reader["Genre"]?.ToString() ?? ""
                            }
                        };
                        userBooks.Add(userBook);
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
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i) == "OwnershipType")
                    {
                        return reader["OwnershipType"]?.ToString() ?? "Physical";
                    }
                }
                bool isOwned = reader["IsOwned"] != DBNull.Value ? (bool)reader["IsOwned"] : true;
                return isOwned ? "Physical" : "Wishlist";
            }
            catch
            {
                return "Physical";
            }
        }
        public bool UpdateBookStatus(int userId, int bookId, string newStatus, DateTime? dateStarted = null, DateTime? dateFinished = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string getCurrentStatusQuery = "SELECT Status FROM UserBooks WHERE UserId = @UserId AND BookId = @BookId";
                    string currentStatus = "";
                    using (SqlCommand getCurrentCmd = new SqlCommand(getCurrentStatusQuery, conn))
                    {
                        getCurrentCmd.Parameters.AddWithValue("@UserId", userId);
                        getCurrentCmd.Parameters.AddWithValue("@BookId", bookId);
                        var result = getCurrentCmd.ExecuteScalar();
                        currentStatus = result?.ToString() ?? "";
                    }
                    bool hasNewColumns = CheckNewColumnsExist(conn);
                    string updateQuery;
                    if (hasNewColumns)
                    {
                        updateQuery = @"
                            UPDATE UserBooks 
                            SET Status = @Status, 
                                DateStarted = @DateStarted,
                                DateFinished = @DateFinished
                            WHERE UserId = @UserId AND BookId = @BookId";
                    }
                    else
                    {
                        updateQuery = @"
                            UPDATE UserBooks 
                            SET Status = @Status
                            WHERE UserId = @UserId AND BookId = @BookId";
                    }
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@UserId", userId);
                        updateCmd.Parameters.AddWithValue("@BookId", bookId);
                        updateCmd.Parameters.AddWithValue("@Status", newStatus);
                        if (hasNewColumns)
                        {
                            updateCmd.Parameters.AddWithValue("@DateStarted", (object?)dateStarted ?? DBNull.Value);
                            updateCmd.Parameters.AddWithValue("@DateFinished", (object?)dateFinished ?? DBNull.Value);
                        }
                        int result = updateCmd.ExecuteNonQuery();
                        if (result > 0 && newStatus == "Read" && currentStatus != "Read")
                        {
                            UpdateReadingGoalProgress(userId, conn);
                        }
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating book status: {ex.Message}", ex);
            }
        }
        public bool UpdateReadingProgress(int userId, int bookId, int? currentPage = null, decimal? readingProgress = null, int? totalPages = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    bool hasNewColumns = CheckNewColumnsExist(conn);
                    if (!hasNewColumns)
                    {
                        return false;
                    }
                    string updateQuery = @"
                        UPDATE UserBooks 
                        SET CurrentPage = @CurrentPage,
                            ReadingProgress = @ReadingProgress,
                            TotalPages = @TotalPages
                        WHERE UserId = @UserId AND BookId = @BookId";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@BookId", bookId);
                        cmd.Parameters.AddWithValue("@CurrentPage", (object?)currentPage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ReadingProgress", (object?)readingProgress ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalPages", (object?)totalPages ?? DBNull.Value);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating reading progress: {ex.Message}", ex);
            }
        }
        private bool CheckNewColumnsExist(SqlConnection conn)
        {
            try
            {
                string checkQuery = @"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'UserBooks' 
                    AND COLUMN_NAME IN ('DateStarted', 'DateFinished', 'CurrentPage', 'ReadingProgress', 'TotalPages')";
                using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                {
                    int columnCount = (int)cmd.ExecuteScalar();
                    return columnCount >= 3;
                }
            }
            catch
            {
                return false;
            }
        }
        private void UpdateReadingGoalProgress(int userId, SqlConnection conn)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                string checkGoalQuery = "SELECT ReadingGoalId, BooksRead FROM ReadingGoals WHERE UserId = @UserId AND Year = @Year";
                using (SqlCommand checkCmd = new SqlCommand(checkGoalQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                    checkCmd.Parameters.AddWithValue("@Year", currentYear);
                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            object goalIdObj = reader["ReadingGoalId"];
                            object currentBooksReadObj = reader["BooksRead"];
                            reader.Close();
                            int goalId = Convert.ToInt32(goalIdObj);
                            int currentBooksRead = Convert.ToInt32(currentBooksReadObj);
                            string updateGoalQuery = "UPDATE ReadingGoals SET BooksRead = @BooksRead WHERE ReadingGoalId = @GoalId";
                            using (SqlCommand updateCmd = new SqlCommand(updateGoalQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@BooksRead", currentBooksRead + 1);
                                updateCmd.Parameters.AddWithValue("@GoalId", goalId);
                                updateCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
using Microsoft.Data.SqlClient;
namespace BookHub.DAL
{
    public class FriendDAL
    {
        private readonly string _connectionString;
        public FriendDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<User> SearchUsers(string searchTerm, int currentUserId)
        {
            var users = new List<User>();
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return users;
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT UserId, Name, Email, Bio, ProfileImage 
                    FROM Users 
                    WHERE (Name LIKE @SearchTerm OR Email LIKE @SearchTerm) 
                    AND UserId != @CurrentUserId", conn);
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                cmd.Parameters.AddWithValue("@CurrentUserId", currentUserId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"]?.ToString() ?? "",
                        Email = reader["Email"]?.ToString() ?? "",
                        Bio = reader["Bio"]?.ToString() ?? "",
                        ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png"
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while searching users: {ex.Message}", ex);
            }
            return users;
        }
        public bool SendFriendRequest(int fromUserId, int toUserId)
        {
            try
            {
                if (fromUserId == toUserId)
                    return false;
                if (FriendRequestExists(fromUserId, toUserId) || AreFriends(fromUserId, toUserId))
                    return false;
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    INSERT INTO FriendRequests (FromUserId, ToUserId, RequestDate, Status) 
                    VALUES (@FromUserId, @ToUserId, @RequestDate, 'Pending')", conn);
                cmd.Parameters.AddWithValue("@FromUserId", fromUserId);
                cmd.Parameters.AddWithValue("@ToUserId", toUserId);
                cmd.Parameters.AddWithValue("@RequestDate", DateTime.Now);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while sending friend request: {ex.Message}", ex);
            }
        }
        public bool FriendRequestExists(int fromUserId, int toUserId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM FriendRequests 
                    WHERE ((FromUserId = @FromUserId AND ToUserId = @ToUserId) 
                           OR (FromUserId = @ToUserId AND ToUserId = @FromUserId)) 
                    AND Status = 'Pending'", conn);
                cmd.Parameters.AddWithValue("@FromUserId", fromUserId);
                cmd.Parameters.AddWithValue("@ToUserId", toUserId);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while checking friend request: {ex.Message}", ex);
            }
        }
        public bool AreFriends(int userId1, int userId2)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Friends 
                    WHERE (UserId = @UserId1 AND FriendUserId = @UserId2) 
                       OR (UserId = @UserId2 AND FriendUserId = @UserId1)", conn);
                cmd.Parameters.AddWithValue("@UserId1", userId1);
                cmd.Parameters.AddWithValue("@UserId2", userId2);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while checking friendship: {ex.Message}", ex);
            }
        }
        public List<FriendRequest> GetPendingRequests(int userId)
        {
            var requests = new List<FriendRequest>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT fr.RequestId, fr.FromUserId, fr.ToUserId, fr.RequestDate, fr.Status,
                           u.Name as FromUserName, u.Email as FromUserEmail, u.Bio as FromUserBio, u.ProfileImage as FromUserImage
                    FROM FriendRequests fr
                    JOIN Users u ON fr.FromUserId = u.UserId
                    WHERE fr.ToUserId = @UserId AND fr.Status = 'Pending'
                    ORDER BY fr.RequestDate DESC", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    requests.Add(new FriendRequest
                    {
                        RequestId = (int)reader["RequestId"],
                        FromUserId = (int)reader["FromUserId"],
                        ToUserId = (int)reader["ToUserId"],
                        RequestDate = (DateTime)reader["RequestDate"],
                        Status = reader["Status"]?.ToString() ?? "Pending",
                        FromUser = new User
                        {
                            UserId = (int)reader["FromUserId"],
                            Name = reader["FromUserName"]?.ToString() ?? "",
                            Email = reader["FromUserEmail"]?.ToString() ?? "",
                            Bio = reader["FromUserBio"]?.ToString() ?? "",
                            ProfileImage = reader["FromUserImage"]?.ToString() ?? "default.png"
                        }
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while getting pending requests: {ex.Message}", ex);
            }
            return requests;
        }
        public bool AcceptFriendRequest(int requestId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();
                try
                {
                    var getRequestCmd = new SqlCommand(@"
                        SELECT FromUserId, ToUserId FROM FriendRequests 
                        WHERE RequestId = @RequestId AND ToUserId = @UserId AND Status = 'Pending'", conn, transaction);
                    getRequestCmd.Parameters.AddWithValue("@RequestId", requestId);
                    getRequestCmd.Parameters.AddWithValue("@UserId", userId);
                    using var reader = getRequestCmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        transaction.Rollback();
                        return false;
                    }
                    int fromUserId = (int)reader["FromUserId"];
                    int toUserId = (int)reader["ToUserId"];
                    reader.Close();
                    var updateRequestCmd = new SqlCommand(@"
                        UPDATE FriendRequests 
                        SET Status = 'Accepted', ResponseDate = @ResponseDate 
                        WHERE RequestId = @RequestId", conn, transaction);
                    updateRequestCmd.Parameters.AddWithValue("@RequestId", requestId);
                    updateRequestCmd.Parameters.AddWithValue("@ResponseDate", DateTime.Now);
                    updateRequestCmd.ExecuteNonQuery();
                    var addFriend1Cmd = new SqlCommand(@"
                        INSERT INTO Friends (UserId, FriendUserId, FriendsSince) 
                        VALUES (@UserId, @FriendUserId, @FriendsSince)", conn, transaction);
                    addFriend1Cmd.Parameters.AddWithValue("@UserId", fromUserId);
                    addFriend1Cmd.Parameters.AddWithValue("@FriendUserId", toUserId);
                    addFriend1Cmd.Parameters.AddWithValue("@FriendsSince", DateTime.Now);
                    addFriend1Cmd.ExecuteNonQuery();
                    var addFriend2Cmd = new SqlCommand(@"
                        INSERT INTO Friends (UserId, FriendUserId, FriendsSince) 
                        VALUES (@UserId, @FriendUserId, @FriendsSince)", conn, transaction);
                    addFriend2Cmd.Parameters.AddWithValue("@UserId", toUserId);
                    addFriend2Cmd.Parameters.AddWithValue("@FriendUserId", fromUserId);
                    addFriend2Cmd.Parameters.AddWithValue("@FriendsSince", DateTime.Now);
                    addFriend2Cmd.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while accepting friend request: {ex.Message}", ex);
            }
        }
        public bool DeclineFriendRequest(int requestId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    UPDATE FriendRequests 
                    SET Status = 'Declined', ResponseDate = @ResponseDate 
                    WHERE RequestId = @RequestId AND ToUserId = @UserId AND Status = 'Pending'", conn);
                cmd.Parameters.AddWithValue("@RequestId", requestId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@ResponseDate", DateTime.Now);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while declining friend request: {ex.Message}", ex);
            }
        }
        public List<User> GetFriends(int userId)
        {
            var friends = new List<User>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT u.UserId, u.Name, u.Email, u.Bio, u.ProfileImage, f.FriendsSince
                    FROM Friends f
                    JOIN Users u ON f.FriendUserId = u.UserId
                    WHERE f.UserId = @UserId
                    ORDER BY f.FriendsSince DESC", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    friends.Add(new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"]?.ToString() ?? "",
                        Email = reader["Email"]?.ToString() ?? "",
                        Bio = reader["Bio"]?.ToString() ?? "",
                        ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png"
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while getting friends: {ex.Message}", ex);
            }
            return friends;
        }
        public List<dynamic> GetFriendsActivity(int userId)
        {
            var activities = new List<dynamic>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT 
                        u.Name as UserName,
                        u.ProfileImage,
                        b.Title as BookTitle,
                        b.Author as BookAuthor,
                        b.CoverUrl as CoverImage,
                        br.Rating,
                        br.ReviewTitle,
                        br.ReviewText,
                        br.ReviewDate,
                        'review' as ActivityType
                    FROM Friends f
                    JOIN Users u ON f.FriendUserId = u.UserId
                    JOIN BookReviews br ON u.UserId = br.UserId
                    JOIN Books b ON br.BookId = b.BookId
                    WHERE f.UserId = @UserId
                    UNION ALL
                    SELECT 
                        u.Name as UserName,
                        u.ProfileImage,
                        b.Title as BookTitle,
                        b.Author as BookAuthor,
                        b.CoverUrl as CoverImage,
                        NULL as Rating,
                        NULL as ReviewTitle,
                        NULL as ReviewText,
                        ub.DateAdded as ReviewDate,
                        'added_to_shelf' as ActivityType
                    FROM Friends f
                    JOIN Users u ON f.FriendUserId = u.UserId
                    JOIN UserBooks ub ON u.UserId = ub.UserId
                    JOIN Books b ON ub.BookId = b.BookId
                    WHERE f.UserId = @UserId
                    ORDER BY ReviewDate DESC", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    activities.Add(new
                    {
                        UserName = reader["UserName"]?.ToString() ?? "",
                        ProfileImage = reader["ProfileImage"]?.ToString() ?? "default.png",
                        BookTitle = reader["BookTitle"]?.ToString() ?? "",
                        BookAuthor = reader["BookAuthor"]?.ToString() ?? "",
                        CoverImage = reader["CoverImage"]?.ToString() ?? "",
                        Rating = reader["Rating"] == DBNull.Value ? (int?)null : (int)reader["Rating"],
                        ReviewTitle = reader["ReviewTitle"]?.ToString() ?? "",
                        ReviewText = reader["ReviewText"]?.ToString() ?? "",
                        ActivityDate = (DateTime)reader["ReviewDate"],
                        ActivityType = reader["ActivityType"]?.ToString() ?? ""
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while getting friends activity: {ex.Message}", ex);
            }
            return activities;
        }
        public bool RemoveFriend(int userId, int friendUserId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    DELETE FROM Friends 
                    WHERE (UserId = @UserId AND FriendUserId = @FriendUserId) 
                       OR (UserId = @FriendUserId AND FriendUserId = @UserId)", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@FriendUserId", friendUserId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while removing friend: {ex.Message}", ex);
            }
        }
    }
}
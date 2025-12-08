using Microsoft.Data.SqlClient;
using BookHub.DAL.Interfaces;
namespace BookHub.DAL
{
    public class BookClubDAL_Simple : IBookClubDAL
    {
        private readonly string _connectionString;
        public BookClubDAL_Simple(string connectionString)
        {
            _connectionString = connectionString;
        }
        public int CreateBookClub(BookClub bookClub)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();
                try
                {
                    var insertClubCmd = new SqlCommand(@"
                        INSERT INTO BookClubs (Name, Description, OwnerId)
                        VALUES (@Name, @Description, @OwnerId);
                        SELECT SCOPE_IDENTITY();", conn, transaction);
                    insertClubCmd.Parameters.AddWithValue("@Name", bookClub.Name);
                    insertClubCmd.Parameters.AddWithValue("@Description", bookClub.Description ?? "");
                    insertClubCmd.Parameters.AddWithValue("@OwnerId", bookClub.OwnerId);
                    int clubId = Convert.ToInt32(insertClubCmd.ExecuteScalar());
                    var insertMemberCmd = new SqlCommand(@"
                        INSERT INTO ClubMemberships (ClubId, UserId, IsApproved, JoinedDate, Role)
                        VALUES (@ClubId, @UserId, 1, GETDATE(), 'Creator')", conn, transaction);
                    insertMemberCmd.Parameters.AddWithValue("@ClubId", clubId);
                    insertMemberCmd.Parameters.AddWithValue("@UserId", bookClub.OwnerId);
                    insertMemberCmd.ExecuteNonQuery();
                    transaction.Commit();
                    return clubId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating book club: {ex.Message}", ex);
            }
        }
        public List<BookClub> GetAllBookClubs()
        {
            var bookClubs = new List<BookClub>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT bc.ClubId, bc.Name, bc.Description, bc.OwnerId,
                           u.Name as OwnerName, u.Email as OwnerEmail,
                           COUNT(CASE WHEN cm.IsApproved = 1 THEN 1 END) as MemberCount
                    FROM BookClubs bc
                    LEFT JOIN Users u ON bc.OwnerId = u.UserId
                    LEFT JOIN ClubMemberships cm ON bc.ClubId = cm.ClubId
                    GROUP BY bc.ClubId, bc.Name, bc.Description, bc.OwnerId, u.Name, u.Email
                    ORDER BY bc.Name", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bookClubs.Add(new BookClub
                    {
                        ClubId = (int)reader["ClubId"],
                        Name = reader["Name"]?.ToString() ?? "",
                        Description = reader["Description"]?.ToString() ?? "",
                        OwnerId = (int)reader["OwnerId"],
                        MemberCount = (int)reader["MemberCount"],
                        Owner = new User
                        {
                            UserId = (int)reader["OwnerId"],
                            Name = reader["OwnerName"]?.ToString() ?? "",
                            Email = reader["OwnerEmail"]?.ToString() ?? ""
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book clubs: {ex.Message}", ex);
            }
            return bookClubs;
        }
        public List<BookClub> GetUserBookClubs(int userId)
        {
            var bookClubs = new List<BookClub>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT bc.ClubId, bc.Name, bc.Description, bc.OwnerId,
                           u.Name as OwnerName, u.Email as OwnerEmail,
                           cm.Role, cm.IsApproved
                    FROM BookClubs bc
                    INNER JOIN ClubMemberships cm ON bc.ClubId = cm.ClubId
                    LEFT JOIN Users u ON bc.OwnerId = u.UserId
                    WHERE cm.UserId = @UserId AND cm.IsApproved = 1
                    ORDER BY bc.Name", conn);
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bookClubs.Add(new BookClub
                    {
                        ClubId = (int)reader["ClubId"],
                        Name = reader["Name"]?.ToString() ?? "",
                        Description = reader["Description"]?.ToString() ?? "",
                        OwnerId = (int)reader["OwnerId"],
                        Owner = new User
                        {
                            UserId = (int)reader["OwnerId"],
                            Name = reader["OwnerName"]?.ToString() ?? "",
                            Email = reader["OwnerEmail"]?.ToString() ?? ""
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting user book clubs: {ex.Message}", ex);
            }
            return bookClubs;
        }
        public bool JoinBookClub(int clubId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var checkCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND UserId = @UserId", conn);
                checkCmd.Parameters.AddWithValue("@ClubId", clubId);
                checkCmd.Parameters.AddWithValue("@UserId", userId);
                int existingCount = (int)checkCmd.ExecuteScalar();
                if (existingCount > 0)
                {
                    return false;
                }
                var insertCmd = new SqlCommand(@"
                    INSERT INTO ClubMemberships (ClubId, UserId, IsApproved, JoinedDate, Role)
                    VALUES (@ClubId, @UserId, 0, GETDATE(), 'Member')", conn);
                insertCmd.Parameters.AddWithValue("@ClubId", clubId);
                insertCmd.Parameters.AddWithValue("@UserId", userId);
                return insertCmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error joining book club: {ex.Message}", ex);
            }
        }
        public bool LeaveBookClub(int clubId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var cmd = new SqlCommand(@"
                    DELETE FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error leaving book club: {ex.Message}", ex);
            }
        }
        public bool IsUserMember(int clubId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND UserId = @UserId AND IsApproved = 1", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return (int)cmd.ExecuteScalar() > 0;
            }
            catch
            {
                return false;
            }
        }
        public int GetBookClubMemberCount(int clubId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND IsApproved = 1", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                return (int)cmd.ExecuteScalar();
            }
            catch
            {
                return 0;
            }
        }
        public bool HasPendingRequest(int clubId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND UserId = @UserId AND IsApproved = 0", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return (int)cmd.ExecuteScalar() > 0;
            }
            catch
            {
                return false;
            }
        }
        public List<BookClub> SearchBookClubs(string searchTerm)
        {
            var bookClubs = new List<BookClub>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT bc.ClubId, bc.Name, bc.Description, bc.OwnerId,
                           u.Name as OwnerName, u.Email as OwnerEmail,
                           COUNT(cm.ClubMembershipId) as MemberCount
                    FROM BookClubs bc
                    LEFT JOIN Users u ON bc.OwnerId = u.UserId
                    LEFT JOIN ClubMemberships cm ON bc.ClubId = cm.ClubId
                    WHERE bc.Name LIKE @SearchTerm OR bc.Description LIKE @SearchTerm
                    GROUP BY bc.ClubId, bc.Name, bc.Description, bc.OwnerId, u.Name, u.Email
                    ORDER BY bc.Name", conn);
                cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bookClubs.Add(new BookClub
                    {
                        ClubId = (int)reader["ClubId"],
                        Name = reader["Name"]?.ToString() ?? "",
                        Description = reader["Description"]?.ToString() ?? "",
                        OwnerId = (int)reader["OwnerId"],
                        MemberCount = (int)reader["MemberCount"],
                        Owner = new User
                        {
                            UserId = (int)reader["OwnerId"],
                            Name = reader["OwnerName"]?.ToString() ?? "",
                            Email = reader["OwnerEmail"]?.ToString() ?? ""
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching book clubs: {ex.Message}", ex);
            }
            return bookClubs;
        }
        public string GetMembershipStatus(int clubId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var ownerCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM BookClubs 
                    WHERE ClubId = @ClubId AND OwnerId = @UserId", conn);
                ownerCmd.Parameters.AddWithValue("@ClubId", clubId);
                ownerCmd.Parameters.AddWithValue("@UserId", userId);
                if ((int)ownerCmd.ExecuteScalar() > 0)
                    return "Creator";
                var memberCmd = new SqlCommand(@"
                    SELECT IsApproved FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND UserId = @UserId", conn);
                memberCmd.Parameters.AddWithValue("@ClubId", clubId);
                memberCmd.Parameters.AddWithValue("@UserId", userId);
                var result = memberCmd.ExecuteScalar();
                if (result != null)
                {
                    bool isApproved = (bool)result;
                    return isApproved ? "Member" : "Pending";
                }
                return "NotMember";
            }
            catch
            {
                return "Unknown";
            }
        }
        public BookClub? GetBookClubById(int clubId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT bc.ClubId, bc.Name, bc.Description, bc.OwnerId,
                           u.Name as OwnerName, u.Email as OwnerEmail
                    FROM BookClubs bc
                    LEFT JOIN Users u ON bc.OwnerId = u.UserId
                    WHERE bc.ClubId = @ClubId", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new BookClub
                    {
                        ClubId = (int)reader["ClubId"],
                        Name = reader["Name"]?.ToString() ?? "",
                        Description = reader["Description"]?.ToString() ?? "",
                        OwnerId = (int)reader["OwnerId"],
                        Owner = new User
                        {
                            UserId = (int)reader["OwnerId"],
                            Name = reader["OwnerName"]?.ToString() ?? "",
                            Email = reader["OwnerEmail"]?.ToString() ?? ""
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book club: {ex.Message}", ex);
            }
            return null;
        }
        public List<BookClubMember> GetBookClubMembers(int clubId)
        {
            var members = new List<BookClubMember>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT cm.ClubMembershipId, cm.ClubId, cm.UserId, cm.IsApproved, cm.JoinedDate, cm.Role,
                           u.Name as UserName, u.Email as UserEmail
                    FROM ClubMemberships cm
                    INNER JOIN Users u ON cm.UserId = u.UserId
                    WHERE cm.ClubId = @ClubId
                    ORDER BY cm.Role DESC, cm.JoinedDate", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    members.Add(new BookClubMember
                    {
                        ClubMembershipId = (int)reader["ClubMembershipId"],
                        ClubId = (int)reader["ClubId"],
                        UserId = (int)reader["UserId"],
                        IsApproved = (bool)reader["IsApproved"],
                        JoinedDate = (DateTime)reader["JoinedDate"],
                        Role = reader["Role"]?.ToString() ?? "Member",
                        User = new User
                        {
                            UserId = (int)reader["UserId"],
                            Name = reader["UserName"]?.ToString() ?? "",
                            Email = reader["UserEmail"]?.ToString() ?? ""
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book club members: {ex.Message}", ex);
            }
            return members;
        }
        public List<BookClubMember> GetPendingMembers(int clubId)
        {
            var members = new List<BookClubMember>();
            try
            {
                using var conn = new SqlConnection(_connectionString);
                var cmd = new SqlCommand(@"
                    SELECT cm.ClubMembershipId, cm.ClubId, cm.UserId, cm.IsApproved, cm.JoinedDate, cm.Role,
                           u.Name as UserName, u.Email as UserEmail
                    FROM ClubMemberships cm
                    INNER JOIN Users u ON cm.UserId = u.UserId
                    WHERE cm.ClubId = @ClubId AND cm.IsApproved = 0
                    ORDER BY cm.JoinedDate", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    members.Add(new BookClubMember
                    {
                        ClubMembershipId = (int)reader["ClubMembershipId"],
                        ClubId = (int)reader["ClubId"],
                        UserId = (int)reader["UserId"],
                        IsApproved = (bool)reader["IsApproved"],
                        JoinedDate = (DateTime)reader["JoinedDate"],
                        Role = reader["Role"]?.ToString() ?? "Member",
                        User = new User
                        {
                            UserId = (int)reader["UserId"],
                            Name = reader["UserName"]?.ToString() ?? "",
                            Email = reader["UserEmail"]?.ToString() ?? ""
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting pending members: {ex.Message}", ex);
            }
            return members;
        }
        public bool ApproveMember(int clubId, int userId, int adminUserId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                if (!IsUserAdmin(clubId, adminUserId, conn))
                    throw new UnauthorizedAccessException("User does not have admin privileges for this club");
                var cmd = new SqlCommand(@"
                    UPDATE ClubMemberships 
                    SET IsApproved = 1 
                    WHERE ClubId = @ClubId AND UserId = @UserId AND IsApproved = 0", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error approving member: {ex.Message}", ex);
            }
        }
        public bool RemoveMember(int clubId, int userId, int adminUserId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                if (!IsUserAdmin(clubId, adminUserId, conn))
                    throw new UnauthorizedAccessException("User does not have admin privileges for this club");
                var ownerCheckCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM BookClubs 
                    WHERE ClubId = @ClubId AND OwnerId = @UserId", conn);
                ownerCheckCmd.Parameters.AddWithValue("@ClubId", clubId);
                ownerCheckCmd.Parameters.AddWithValue("@UserId", userId);
                if ((int)ownerCheckCmd.ExecuteScalar() > 0)
                    throw new InvalidOperationException("Cannot remove the club owner");
                var cmd = new SqlCommand(@"
                    DELETE FROM ClubMemberships 
                    WHERE ClubId = @ClubId AND UserId = @UserId", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error removing member: {ex.Message}", ex);
            }
        }
        public bool ChangeMemberRole(int clubId, int userId, string newRole, int adminUserId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                var ownerCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM BookClubs 
                    WHERE ClubId = @ClubId AND OwnerId = @AdminUserId", conn);
                ownerCmd.Parameters.AddWithValue("@ClubId", clubId);
                ownerCmd.Parameters.AddWithValue("@AdminUserId", adminUserId);
                if ((int)ownerCmd.ExecuteScalar() == 0)
                    throw new UnauthorizedAccessException("Only club owners can change member roles");
                var validRoles = new[] { "Member", "Moderator", "Admin" };
                if (!validRoles.Contains(newRole))
                    throw new ArgumentException("Invalid role specified");
                var cmd = new SqlCommand(@"
                    UPDATE ClubMemberships 
                    SET Role = @Role 
                    WHERE ClubId = @ClubId AND UserId = @UserId AND IsApproved = 1", conn);
                cmd.Parameters.AddWithValue("@Role", newRole);
                cmd.Parameters.AddWithValue("@ClubId", clubId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error changing member role: {ex.Message}", ex);
            }
        }
        private bool IsUserAdmin(int clubId, int userId, SqlConnection conn)
        {
            var ownerCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM BookClubs 
                WHERE ClubId = @ClubId AND OwnerId = @UserId", conn);
            ownerCmd.Parameters.AddWithValue("@ClubId", clubId);
            ownerCmd.Parameters.AddWithValue("@UserId", userId);
            if ((int)ownerCmd.ExecuteScalar() > 0)
                return true;
            var roleCmd = new SqlCommand(@"
                SELECT Role FROM ClubMemberships 
                WHERE ClubId = @ClubId AND UserId = @UserId AND IsApproved = 1", conn);
            roleCmd.Parameters.AddWithValue("@ClubId", clubId);
            roleCmd.Parameters.AddWithValue("@UserId", userId);
            var role = roleCmd.ExecuteScalar()?.ToString();
            return role == "Admin" || role == "Moderator" || role == "Creator";
        }
        public bool IsUserAdmin(int clubId, int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                return IsUserAdmin(clubId, userId, conn);
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateBookClub(BookClub bookClub)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                var cmd = new SqlCommand(@"
                    UPDATE BookClubs 
                    SET Name = @Name, 
                        Description = @Description, 
                        Genre = @Genre, 
                        MeetingSchedule = @MeetingSchedule, 
                        MaxMembers = @MaxMembers
                    WHERE ClubId = @ClubId", conn);

                cmd.Parameters.AddWithValue("@ClubId", bookClub.ClubId);
                cmd.Parameters.AddWithValue("@Name", bookClub.Name);
                cmd.Parameters.AddWithValue("@Description", bookClub.Description ?? "");
                cmd.Parameters.AddWithValue("@Genre", bookClub.Genre ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MeetingSchedule", bookClub.MeetingSchedule ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxMembers", bookClub.MaxMembers);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating book club: {ex.Message}", ex);
            }
        }

        public bool DeleteBookClub(int clubId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                var cmd = new SqlCommand(@"DELETE FROM BookClubs WHERE ClubId = @ClubId", conn);
                cmd.Parameters.AddWithValue("@ClubId", clubId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting book club: {ex.Message}", ex);
            }
        }

    #region Forum Methods
    public List<DiscussionPost> GetDiscussionPosts(int clubId, int skip = 0, int take = 20)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var posts = new List<DiscussionPost>();
            var cmd = new SqlCommand(@"
                SELECT d.DiscussionId, d.BookClubId, d.UserId, d.Message, d.PostedDate, 
                       u.Name as UserName,
                       (SELECT COUNT(*) FROM BookClubDiscussions WHERE ReplyToId = d.DiscussionId) as ReplyCount
                FROM BookClubDiscussions d
                INNER JOIN Users u ON d.UserId = u.UserId
                WHERE d.BookClubId = @ClubId AND d.ReplyToId IS NULL
                ORDER BY d.PostedDate DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", conn);
            cmd.Parameters.AddWithValue("@ClubId", clubId);
            cmd.Parameters.AddWithValue("@Skip", skip);
            cmd.Parameters.AddWithValue("@Take", take);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var post = new DiscussionPost
                {
                    PostId = reader.GetInt32(0),
                    ClubId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    Title = "Discussion",
                    Content = reader.GetString(3),
                    CreatedDate = reader.GetDateTime(4),
                    UpdatedDate = reader.GetDateTime(4),
                    IsSticky = false,
                    ReplyCount = reader.GetInt32(6),
                    User = new User { Name = reader.GetString(5) }
                };
                posts.Add(post);
            }
            return posts;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting discussion posts: {ex.Message}", ex);
        }
    }
    public DiscussionPost? GetDiscussionPost(int postId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var postCmd = new SqlCommand(@"
                SELECT d.DiscussionId, d.BookClubId, d.UserId, d.Message, d.PostedDate, 
                       u.Name as UserName
                FROM BookClubDiscussions d
                INNER JOIN Users u ON d.UserId = u.UserId
                WHERE d.DiscussionId = @PostId", conn);
            postCmd.Parameters.AddWithValue("@PostId", postId);
            DiscussionPost? post = null;
            using var reader = postCmd.ExecuteReader();
            if (reader.Read())
            {
                post = new DiscussionPost
                {
                    PostId = reader.GetInt32(0),
                    ClubId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    Title = "Discussion",
                    Content = reader.GetString(3),
                    CreatedDate = reader.GetDateTime(4),
                    UpdatedDate = reader.GetDateTime(4),
                    IsSticky = false,
                    ReplyCount = 0,
                    User = new User { Name = reader.GetString(5) }
                };
            }
            reader.Close();
            if (post == null) return null;
            var repliesCmd = new SqlCommand(@"
                SELECT d.DiscussionId, d.UserId, d.Message, d.PostedDate, u.Name as UserName
                FROM BookClubDiscussions d
                INNER JOIN Users u ON d.UserId = u.UserId
                WHERE d.ReplyToId = @PostId
                ORDER BY d.PostedDate ASC", conn);
            repliesCmd.Parameters.AddWithValue("@PostId", postId);
            using var repliesReader = repliesCmd.ExecuteReader();
            while (repliesReader.Read())
            {
                var reply = new DiscussionReply
                {
                    ReplyId = repliesReader.GetInt32(0),
                    PostId = postId,
                    UserId = repliesReader.GetInt32(1),
                    Content = repliesReader.GetString(2),
                    CreatedDate = repliesReader.GetDateTime(3),
                    UpdatedDate = repliesReader.GetDateTime(3),
                    User = new User { Name = repliesReader.GetString(4) }
                };
                post.Replies.Add(reply);
            }
            post.ReplyCount = post.Replies.Count;
            return post;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting discussion post: {ex.Message}", ex);
        }
    }
    public int CreateDiscussionPost(DiscussionPost post)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                INSERT INTO BookClubDiscussions (BookClubId, UserId, Message, PostedDate, ReplyToId)
                VALUES (@BookClubId, @UserId, @Message, @PostedDate, NULL);
                SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@BookClubId", post.ClubId);
            cmd.Parameters.AddWithValue("@UserId", post.UserId);
            cmd.Parameters.AddWithValue("@Message", post.Content);
            cmd.Parameters.AddWithValue("@PostedDate", post.CreatedDate);
            int postId = Convert.ToInt32(cmd.ExecuteScalar());
            return postId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating discussion post: {ex.Message}", ex);
        }
    }
    public int CreateDiscussionReply(DiscussionReply reply)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                INSERT INTO BookClubDiscussions (BookClubId, UserId, Message, PostedDate, ReplyToId)
                SELECT BookClubId, @UserId, @Message, @PostedDate, @ReplyToId
                FROM BookClubDiscussions
                WHERE DiscussionId = @ReplyToId;
                SELECT SCOPE_IDENTITY();", conn);
            cmd.Parameters.AddWithValue("@UserId", reply.UserId);
            cmd.Parameters.AddWithValue("@Message", reply.Content);
            cmd.Parameters.AddWithValue("@PostedDate", reply.CreatedDate);
            cmd.Parameters.AddWithValue("@ReplyToId", reply.PostId);
            int replyId = Convert.ToInt32(cmd.ExecuteScalar());
            return replyId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating discussion reply: {ex.Message}", ex);
        }
    }
    public List<PostAttachment> GetAttachments(int? postId = null, int? replyId = null)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            string whereClause = postId.HasValue ? "PostId = @PostId" : "ReplyId = @ReplyId";
            var cmd = new SqlCommand($@"
                SELECT AttachmentId, PostId, ReplyId, FileName, FileType, FilePath, FileSize, UploadedDate
                FROM PostAttachments
                WHERE {whereClause}
                ORDER BY UploadedDate ASC", conn);
            if (postId.HasValue)
                cmd.Parameters.AddWithValue("@PostId", postId.Value);
            else
                cmd.Parameters.AddWithValue("@ReplyId", replyId!.Value);
            var attachments = new List<PostAttachment>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var attachment = new PostAttachment
                {
                    AttachmentId = reader.GetInt32(0),
                    PostId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    ReplyId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    FileName = reader.GetString(3),
                    FileType = reader.GetString(4),
                    FilePath = reader.GetString(5),
                    FileSize = reader.GetInt64(6),
                    UploadedDate = reader.GetDateTime(7)
                };
                attachments.Add(attachment);
            }
            return attachments;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting attachments: {ex.Message}", ex);
        }
    }
    public bool CanUserPostInClub(int clubId, int userId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var ownerCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM BookClubs 
                WHERE ClubId = @ClubId AND OwnerId = @UserId", conn);
            ownerCmd.Parameters.AddWithValue("@ClubId", clubId);
            ownerCmd.Parameters.AddWithValue("@UserId", userId);
            if ((int)ownerCmd.ExecuteScalar() > 0)
                return true;
            var memberCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM ClubMemberships 
                WHERE ClubId = @ClubId AND UserId = @UserId AND IsApproved = 1", conn);
            memberCmd.Parameters.AddWithValue("@ClubId", clubId);
            memberCmd.Parameters.AddWithValue("@UserId", userId);
            return (int)memberCmd.ExecuteScalar() > 0;
        }
        catch
        {
            return false;
        }
    }
    #endregion
    #region Reading Goals
    public List<ReadingGoal> GetUserReadingGoals(int userId)
    {
        var goals = new List<ReadingGoal>();
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var cmd = new SqlCommand(@"
            SELECT ReadingGoalId, UserId, Year, TargetBooks, BooksRead
            FROM ReadingGoals
            WHERE UserId = @UserId
            ORDER BY Year DESC", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            goals.Add(new ReadingGoal
            {
                ReadingGoalId = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Year = reader.GetInt32(2),
                TargetBooks = reader.GetInt32(3),
                BooksRead = reader.GetInt32(4)
            });
        }
        return goals;
    }
    public ReadingGoal? GetUserReadingGoalByYear(int userId, int year)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();
        var cmd = new SqlCommand(@"
            SELECT ReadingGoalId, UserId, Year, TargetBooks, BooksRead
            FROM ReadingGoals
            WHERE UserId = @UserId AND Year = @Year", conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Year", year);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ReadingGoal
            {
                ReadingGoalId = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Year = reader.GetInt32(2),
                TargetBooks = reader.GetInt32(3),
                BooksRead = reader.GetInt32(4)
            };
        }
        return null;
    }
    public int CreateOrUpdateReadingGoal(ReadingGoal goal)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var checkCmd = new SqlCommand(@"
                SELECT ReadingGoalId FROM ReadingGoals
                WHERE UserId = @UserId AND Year = @Year", conn);
            checkCmd.Parameters.AddWithValue("@UserId", goal.UserId);
            checkCmd.Parameters.AddWithValue("@Year", goal.Year);
            var existingId = checkCmd.ExecuteScalar();
            if (existingId != null)
            {
                var updateCmd = new SqlCommand(@"
                    UPDATE ReadingGoals
                    SET TargetBooks = @TargetBooks
                    WHERE ReadingGoalId = @ReadingGoalId", conn);
                updateCmd.Parameters.AddWithValue("@TargetBooks", goal.TargetBooks);
                updateCmd.Parameters.AddWithValue("@ReadingGoalId", existingId);
                updateCmd.ExecuteNonQuery();
                return (int)existingId;
            }
            else
            {
                var insertCmd = new SqlCommand(@"
                    INSERT INTO ReadingGoals (UserId, Year, TargetBooks, BooksRead)
                    VALUES (@UserId, @Year, @TargetBooks, @BooksRead);
                    SELECT SCOPE_IDENTITY();", conn);
                insertCmd.Parameters.AddWithValue("@UserId", goal.UserId);
                insertCmd.Parameters.AddWithValue("@Year", goal.Year);
                insertCmd.Parameters.AddWithValue("@TargetBooks", goal.TargetBooks);
                insertCmd.Parameters.AddWithValue("@BooksRead", goal.BooksRead);
                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
        }
        catch
        {
            return -1;
        }
    }
    public bool UpdateBooksRead(int userId, int year, int booksRead)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand(@"
                UPDATE ReadingGoals
                SET BooksRead = @BooksRead
                WHERE UserId = @UserId AND Year = @Year", conn);
            cmd.Parameters.AddWithValue("@BooksRead", booksRead);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Year", year);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch
        {
            return false;
        }
    }
    public bool DeleteReadingGoal(int readingGoalId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("DELETE FROM ReadingGoals WHERE ReadingGoalId = @ReadingGoalId", conn);
            cmd.Parameters.AddWithValue("@ReadingGoalId", readingGoalId);
            return cmd.ExecuteNonQuery() > 0;
        }
        catch
        {
            return false;
        }
    }
    #endregion
}
}
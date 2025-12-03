using BookHub.DAL;
namespace BookHub.BLL
{
    public class BookClubBLL : IBookClubBLL
    {
        private readonly BookClubDAL_Simple _bookClubDAL;
        public BookClubBLL(string connectionString)
        {
            _bookClubDAL = new BookClubDAL_Simple(connectionString);
        }
        public BookClubBLL(BookClubDAL_Simple bookClubDAL)
        {
            _bookClubDAL = bookClubDAL;
        }
        public int CreateBookClub(BookClub bookClub)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bookClub.Name))
                    throw new ArgumentException("Book club name is required.");
                if (bookClub.Name.Length > 100)
                    throw new ArgumentException("Book club name cannot exceed 100 characters.");
                if (!string.IsNullOrEmpty(bookClub.Description) && bookClub.Description.Length > 500)
                    throw new ArgumentException("Description cannot exceed 500 characters.");
                if (bookClub.OwnerId <= 0)
                    throw new ArgumentException("Invalid owner user ID.");
                if (bookClub.MaxMembers < 2 || bookClub.MaxMembers > 1000)
                    throw new ArgumentException("Maximum members must be between 2 and 1000.");
                bookClub.CreatedDate = DateTime.Now;
                bookClub.IsActive = true;
                return _bookClubDAL.CreateBookClub(bookClub);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating book club: {ex.Message}", ex);
            }
        }
        public List<BookClub> GetAllBookClubs()
        {
            try
            {
                return _bookClubDAL.GetAllBookClubs();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book clubs: {ex.Message}", ex);
            }
        }
        public BookClub? GetBookClubById(int bookClubId)
        {
            try
            {
                if (bookClubId <= 0)
                    throw new ArgumentException("Invalid book club ID.");
                return _bookClubDAL.GetBookClubById(bookClubId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book club: {ex.Message}", ex);
            }
        }
        public List<BookClub> GetUserBookClubs(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");
                return _bookClubDAL.GetUserBookClubs(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting user book clubs: {ex.Message}", ex);
            }
        }
        public bool JoinBookClub(int bookClubId, int userId)
        {
            try
            {
                if (bookClubId <= 0 || userId <= 0)
                    throw new ArgumentException("Invalid book club ID or user ID.");
                if (_bookClubDAL.IsUserMember(bookClubId, userId))
                    throw new InvalidOperationException("User is already a member of this book club.");
                var memberCount = _bookClubDAL.GetBookClubMemberCount(bookClubId);
                var bookClub = _bookClubDAL.GetBookClubById(bookClubId);
                if (memberCount >= (bookClub?.MaxMembers ?? 0))
                    throw new InvalidOperationException("Book club is at maximum capacity.");
                return _bookClubDAL.JoinBookClub(bookClubId, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error joining book club: {ex.Message}", ex);
            }
        }
        public bool LeaveBookClub(int bookClubId, int userId)
        {
            try
            {
                if (bookClubId <= 0 || userId <= 0)
                    throw new ArgumentException("Invalid book club ID or user ID.");
                if (!_bookClubDAL.IsUserMember(bookClubId, userId))
                    throw new InvalidOperationException("User is not a member of this book club.");
                var bookClub = _bookClubDAL.GetBookClubById(bookClubId);
                if (bookClub != null && bookClub.OwnerId == userId)
                    throw new InvalidOperationException("Book club creator cannot leave their own club. Consider transferring ownership or deleting the club.");
                return _bookClubDAL.LeaveBookClub(bookClubId, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error leaving book club: {ex.Message}", ex);
            }
        }
        public bool IsUserMember(int bookClubId, int userId)
        {
            try
            {
                if (bookClubId <= 0 || userId <= 0)
                    return false;
                return _bookClubDAL.IsUserMember(bookClubId, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking membership: {ex.Message}", ex);
            }
        }
        public bool IsBookClubAtCapacity(int bookClubId)
        {
            try
            {
                if (bookClubId <= 0)
                    return true;
                var memberCount = _bookClubDAL.GetBookClubMemberCount(bookClubId);
                var bookClub = _bookClubDAL.GetBookClubById(bookClubId);
                return memberCount >= (bookClub?.MaxMembers ?? 0);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error checking capacity: {ex.Message}", ex);
            }
        }
        public List<BookClubMember> GetBookClubMembers(int bookClubId)
        {
            try
            {
                if (bookClubId <= 0)
                    throw new ArgumentException("Invalid book club ID.");
                return _bookClubDAL.GetBookClubMembers(bookClubId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book club members: {ex.Message}", ex);
            }
        }
        public List<BookClub> SearchBookClubs(string? searchTerm = null, string? genre = null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Length < 2)
                    throw new ArgumentException("Search term must be at least 2 characters long.");
                string combinedSearchTerm = searchTerm?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    combinedSearchTerm = string.IsNullOrEmpty(combinedSearchTerm) ? genre.Trim() : combinedSearchTerm;
                }
                return _bookClubDAL.SearchBookClubs(combinedSearchTerm);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error searching book clubs: {ex.Message}", ex);
            }
        }
        public string GetMembershipStatus(int bookClubId, int userId)
        {
            try
            {
                if (bookClubId <= 0 || userId <= 0)
                    return "Unknown";
                var bookClub = _bookClubDAL.GetBookClubById(bookClubId);
                if (bookClub == null)
                    return "ClubNotFound";
                if (bookClub.OwnerId == userId)
                    return "Creator";
                if (_bookClubDAL.IsUserMember(bookClubId, userId))
                    return "Member";
                if (_bookClubDAL.HasPendingRequest(bookClubId, userId))
                    return "Pending";
                var memberCount = _bookClubDAL.GetBookClubMemberCount(bookClubId);
                if (memberCount >= (bookClub.MaxMembers))
                    return "Full";
                return "NotMember";
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting membership status: {ex.Message}", ex);
            }
        }
        public dynamic? GetBookClubStats(int bookClubId)
        {
            try
            {
                if (bookClubId <= 0)
                    throw new ArgumentException("Invalid book club ID.");
                var members = _bookClubDAL.GetBookClubMembers(bookClubId);
                var bookClub = _bookClubDAL.GetBookClubById(bookClubId);
                if (bookClub == null)
                    return null;
                return new
                {
                    TotalMembers = members.Count,
                    MaxMembers = bookClub.MaxMembers,
                    AvailableSpots = bookClub.MaxMembers - members.Count,
                    CreatedDate = bookClub.CreatedDate,
                    IsAtCapacity = members.Count >= bookClub.MaxMembers,
                    CreatorCount = members.Count(m => m.Role == "Creator"),
                    ModeratorCount = members.Count(m => m.Role == "Moderator"),
                    MemberCount = members.Count(m => m.Role == "Member")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting book club statistics: {ex.Message}", ex);
            }
        }
        public List<string> ValidateBookClub(BookClub bookClub)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(bookClub.Name))
                errors.Add("Book club name is required.");
            else if (bookClub.Name.Length > 100)
                errors.Add("Book club name cannot exceed 100 characters.");
            if (!string.IsNullOrEmpty(bookClub.Description) && bookClub.Description.Length > 500)
                errors.Add("Description cannot exceed 500 characters.");
            if (bookClub.MaxMembers < 2)
                errors.Add("Maximum members must be at least 2.");
            else if (bookClub.MaxMembers > 1000)
                errors.Add("Maximum members cannot exceed 1000.");
            if (!string.IsNullOrEmpty(bookClub.MeetingSchedule) && bookClub.MeetingSchedule.Length > 200)
                errors.Add("Meeting schedule cannot exceed 200 characters.");
            if (!string.IsNullOrEmpty(bookClub.Genre) && bookClub.Genre.Length > 100)
                errors.Add("Genre cannot exceed 100 characters.");
            return errors;
        }
        public List<BookClubMember> GetPendingMembers(int clubId, int adminUserId)
        {
            try
            {
                if (!_bookClubDAL.IsUserAdmin(clubId, adminUserId))
                    throw new UnauthorizedAccessException("You do not have admin privileges for this club.");
                return _bookClubDAL.GetPendingMembers(clubId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting pending members: {ex.Message}", ex);
            }
        }
        public bool ApproveMember(int clubId, int userId, int adminUserId)
        {
            try
            {
                if (clubId <= 0)
                    throw new ArgumentException("Invalid club ID.");
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");
                if (adminUserId <= 0)
                    throw new ArgumentException("Invalid admin user ID.");
                return _bookClubDAL.ApproveMember(clubId, userId, adminUserId);
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
                if (clubId <= 0)
                    throw new ArgumentException("Invalid club ID.");
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");
                if (adminUserId <= 0)
                    throw new ArgumentException("Invalid admin user ID.");
                return _bookClubDAL.RemoveMember(clubId, userId, adminUserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error removing member: {ex.Message}", ex);
            }
        }
        public bool ChangeMemberRole(int clubId, int userId, string newRole, int ownerUserId)
        {
            try
            {
                if (clubId <= 0)
                    throw new ArgumentException("Invalid club ID.");
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID.");
                if (ownerUserId <= 0)
                    throw new ArgumentException("Invalid owner user ID.");
                if (string.IsNullOrWhiteSpace(newRole))
                    throw new ArgumentException("Role is required.");
                var validRoles = new[] { "Member", "Moderator", "Admin" };
                if (!validRoles.Contains(newRole))
                    throw new ArgumentException("Invalid role. Valid roles are: Member, Moderator, Admin.");
                return _bookClubDAL.ChangeMemberRole(clubId, userId, newRole, ownerUserId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error changing member role: {ex.Message}", ex);
            }
        }
        public bool IsUserAdmin(int clubId, int userId)
        {
            try
            {
                if (clubId <= 0 || userId <= 0)
                    return false;
                return _bookClubDAL.IsUserAdmin(clubId, userId);
            }
            catch
            {
                return false;
            }
        }
    }
}
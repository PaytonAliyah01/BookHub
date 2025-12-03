namespace BookHub.BLL
{
    public interface IBookBLL
    {
        List<BookDto> GetAllBooks();
        BookDto? GetBookById(int bookId);
        int AddBook(BookDto bookDto);
    }
    public interface IReadingGoalBLL
    {
        List<ReadingGoalDto> GetUserReadingGoals(int userId);
        ReadingGoalDto? GetCurrentYearGoal(int userId);
        ReadingGoalDto? GetGoalByYear(int userId, int year);
        string SetReadingGoal(int userId, int year, int targetBooks);
        string UpdateProgress(int userId, int year, int booksRead);
        string UpdateGoalTarget(int userId, int year, int newTargetBooks);
        string IncrementProgress(int userId, int year = 0);
        string DecrementProgress(int userId, int year = 0);
        string DeleteReadingGoal(int readingGoalId);
        List<int> GetAvailableYears();
        string GetMotivationalMessage(ReadingGoalDto? goal);
        Dictionary<string, object> GetProgressAnalytics(ReadingGoalDto? goal);
    }
    public interface IUserBookshelfBLL
    {
        bool AddBookToUserBookshelf(int userId, int bookId, string status, string ownershipType);
        List<BookHub.DAL.UserBookshelf> GetUserBookshelf(int userId);
        Dictionary<string, int> GetUserBookshelfStats(int userId);
        Dictionary<string, int> GetUserGenreStats(int userId);
        bool UpdateBookStatus(int userId, int bookId, string newStatus);
        bool UpdateBookStatus(int userId, int bookId, string status, int? rating = null, string notes = "");
        bool UpdateBookStatusWithDates(int userId, int bookId, string newStatus, DateTime? dateStarted = null, DateTime? dateFinished = null);
        bool UpdateReadingProgress(int userId, int bookId, int? currentPage = null, decimal? readingProgress = null, int? totalPages = null);
        bool UpdateOwnershipType(int userId, int bookId, string newOwnershipType);
        bool RemoveBookFromUserBookshelf(int userId, int bookId);
    }
    public interface IBookReviewBLL
    {
        bool AddReview(BookReviewDto reviewDto);
        bool UpdateReview(BookReviewDto reviewDto);
        bool DeleteReview(int reviewId, int userId);
        List<BookReviewDto> GetReviewsForBook(int bookId);
        BookReviewDto? GetUserReviewForBook(int userId, int bookId);
        (double averageRating, int reviewCount) GetBookRatingStats(int bookId);
    }
    public interface IUserBLL
    {
        UserDto? GetUserById(int userId);
        UserDto? GetUserProfile(string email);
        UserDto? GetUserProfile(int userId);
        UserDto? ValidateUser(string email, string password);
        bool UserExists(string email);
        bool RegisterUser(string name, string email, string password);
        bool ChangePassword(string email, string currentPassword, string newPassword);
        bool ResetPassword(string email, string newPassword);
        bool UpdateProfile(string email, string name, string bio, string profileImage);
        bool IsPasswordStrong(string password);
        string GenerateTemporaryPassword(int length);
        string GenerateProfileImageFileName(string originalFileName, int userId);
    }
    public interface IBookClubBLL
    {
        int CreateBookClub(BookHub.DAL.BookClub bookClub);
        List<BookHub.DAL.BookClub> GetAllBookClubs();
        BookHub.DAL.BookClub? GetBookClubById(int bookClubId);
        List<BookHub.DAL.BookClub> GetUserBookClubs(int userId);
        bool JoinBookClub(int bookClubId, int userId);
        bool LeaveBookClub(int bookClubId, int userId);
        List<BookHub.DAL.BookClub> SearchBookClubs(string? searchTerm = null, string? genre = null);
        string GetMembershipStatus(int bookClubId, int userId);
        List<string> ValidateBookClub(BookHub.DAL.BookClub bookClub);
        bool ApproveMember(int clubId, int userId, int adminUserId);
        bool RemoveMember(int clubId, int userId, int adminUserId);
        bool ChangeMemberRole(int clubId, int userId, string newRole, int ownerUserId);
        bool IsUserAdmin(int clubId, int userId);
        List<BookHub.DAL.BookClubMember> GetBookClubMembers(int clubId);
        List<BookHub.DAL.BookClubMember> GetPendingMembers(int clubId, int adminUserId);
    }
    public interface IForumBLL
    {
        bool CanUserPostInClub(int clubId, int userId);
        BookHub.DAL.DiscussionPost? GetDiscussionPost(int postId);
        List<BookHub.DAL.DiscussionPost> GetDiscussionPosts(int clubId, int page, int pageSize = 10);
        int CreateDiscussionPost(int clubId, int userId, string title, string content, bool isSticky = false, List<BookHub.DAL.PostAttachment>? attachments = null);
        int CreateDiscussionReply(int postId, int userId, string content, List<BookHub.DAL.PostAttachment>? attachments = null);
    }
    public interface IFriendBLL
    {
        List<BookHub.DAL.User> SearchUsers(string searchTerm, int currentUserId);
        string GetRelationshipStatus(int currentUserId, int targetUserId);
        bool SendFriendRequest(int fromUserId, int toUserId);
        bool AcceptFriendRequest(int requestId, int userId);
        bool DeclineFriendRequest(int requestId, int userId);
        bool RemoveFriend(int userId, int friendUserId);
        List<BookHub.DAL.User> GetFriends(int userId);
        List<BookHub.DAL.FriendRequest> GetPendingRequests(int userId);
        List<dynamic> GetFriendsActivity(int userId, int limit);
    }
    public interface IAnalyticsBLL
    {
        Dictionary<string, object> GetReadingProgressData(int userId);
        Dictionary<string, object> GetReviewAnalytics(int userId);
        Dictionary<string, object> GetBookshelfAnalytics(int userId);
    }
}
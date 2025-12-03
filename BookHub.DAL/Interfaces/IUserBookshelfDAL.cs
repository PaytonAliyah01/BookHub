namespace BookHub.DAL.Interfaces
{
    public interface IUserBookshelfDAL
    {
        bool AddBookToUserBookshelf(int userId, int bookId, string status = "Want to Read", string ownershipType = "Physical");
        List<UserBookshelf> GetUserBookshelf(int userId);
        Dictionary<string, int> GetUserBookshelfStats(int userId);
        bool UpdateBookStatus(int userId, int bookId, string newStatus, DateTime? dateStarted = null, DateTime? dateFinished = null);
        bool UpdateReadingProgress(int userId, int bookId, int? currentPage, decimal? readingProgress, int? totalPages);
        bool RemoveBookFromUserBookshelf(int userId, int bookId);
    }
}
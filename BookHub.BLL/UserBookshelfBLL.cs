using BookHub.DAL;
namespace BookHub.BLL
{
    public class UserBookshelfBLL : IUserBookshelfBLL
    {
        private readonly UserBookshelfDAL _userBookshelfDAL;
        private readonly BookDAL _bookDAL;
        public UserBookshelfBLL(string connectionString)
        {
            _userBookshelfDAL = new UserBookshelfDAL(connectionString);
            _bookDAL = new BookDAL(connectionString);
        }
        public bool AddBookToUserBookshelf(int userId, int bookId, string status = "Want to Read", string ownershipType = "Physical")
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                var validStatuses = new[] { "Want to Read", "Reading", "Read" };
                if (!validStatuses.Contains(status))
                    status = "Want to Read";
                var book = _bookDAL.GetBookById(bookId);
                if (book == null)
                    throw new ArgumentException("Book not found");
                if (_userBookshelfDAL.IsBookInUserBookshelf(userId, bookId))
                    return false; 
                return _userBookshelfDAL.AddBookToUserBookshelf(userId, bookId, status, ownershipType);
            }
            catch (ArgumentException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error adding book to bookshelf: {ex.Message}", ex);
            }
        }
        public List<UserBookshelf> GetUserBookshelf(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                return _userBookshelfDAL.GetUserBookshelf(userId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving user bookshelf: {ex.Message}", ex);
            }
        }
        public List<UserBookshelf> GetBooksByStatus(int userId, string status)
        {
            try
            {
                var allBooks = GetUserBookshelf(userId);
                return allBooks.Where(ub => ub.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving books by status: {ex.Message}", ex);
            }
        }
        public bool UpdateBookStatus(int userId, int bookId, string newStatus)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                var validStatuses = new[] { "Want to Read", "Reading", "Read" };
                if (!validStatuses.Contains(newStatus))
                    throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
                return _userBookshelfDAL.UpdateBookStatus(userId, bookId, newStatus);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating book status: {ex.Message}", ex);
            }
        }
        public bool UpdateOwnershipType(int userId, int bookId, string newOwnershipType)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                var validOwnershipTypes = new[] { "Physical", "Digital", "Wishlist" };
                if (!validOwnershipTypes.Contains(newOwnershipType))
                    throw new ArgumentException($"Invalid ownership type. Valid types are: {string.Join(", ", validOwnershipTypes)}");
                return _userBookshelfDAL.UpdateOwnershipType(userId, bookId, newOwnershipType);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating ownership type: {ex.Message}", ex);
            }
        }
        public bool RemoveBookFromUserBookshelf(int userId, int bookId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                return _userBookshelfDAL.RemoveBookFromUserBookshelf(userId, bookId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error removing book from bookshelf: {ex.Message}", ex);
            }
        }
        public bool UpdateBookStatus(int userId, int bookId, string status, int? rating = null, string notes = "")
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                var validStatuses = new[] { "Want to Read", "Reading", "Read" };
                if (!validStatuses.Contains(status))
                    throw new ArgumentException("Invalid status");
                if (rating.HasValue && (rating.Value < 1 || rating.Value > 5))
                    throw new ArgumentException("Rating must be between 1 and 5");
                return _userBookshelfDAL.UpdateBookStatus(userId, bookId, status, rating, notes);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating book status: {ex.Message}", ex);
            }
        }
        public bool IsBookInUserBookshelf(int userId, int bookId)
        {
            try
            {
                if (userId <= 0 || bookId <= 0)
                    return false;
                return _userBookshelfDAL.IsBookInUserBookshelf(userId, bookId);
            }
            catch (Exception)
            {
                return false; 
            }
        }
        public Dictionary<string, int> GetUserBookshelfStats(int userId)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                return _userBookshelfDAL.GetUserBookshelfStats(userId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving bookshelf statistics: {ex.Message}", ex);
            }
        }
        public Dictionary<string, int> GetUserGenreStats(int userId)
        {
            try
            {
                var userBooks = GetUserBookshelf(userId);
                return userBooks
                    .Where(ub => ub.Book != null && !string.IsNullOrWhiteSpace(ub.Book.Genre))
                    .GroupBy(ub => ub.Book!.Genre)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving genre statistics: {ex.Message}", ex);
            }
        }
        public bool UpdateBookStatusWithDates(int userId, int bookId, string newStatus, DateTime? dateStarted = null, DateTime? dateFinished = null)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                var validStatuses = new[] { "Want to Read", "Reading", "Read" };
                if (!validStatuses.Contains(newStatus))
                    throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
                if (newStatus == "Reading" && !dateStarted.HasValue)
                {
                    dateStarted = DateTime.Now;
                }
                else if (newStatus == "Read" && !dateFinished.HasValue)
                {
                    dateFinished = DateTime.Now;
                }
                return _userBookshelfDAL.UpdateBookStatus(userId, bookId, newStatus, dateStarted, dateFinished);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating book status with dates: {ex.Message}", ex);
            }
        }
        public bool UpdateReadingProgress(int userId, int bookId, int? currentPage = null, decimal? readingProgress = null, int? totalPages = null)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");
                if (readingProgress.HasValue && (readingProgress < 0 || readingProgress > 100))
                    throw new ArgumentException("Reading progress must be between 0 and 100");
                if (currentPage.HasValue && currentPage < 0)
                    throw new ArgumentException("Current page cannot be negative");
                if (totalPages.HasValue && totalPages < 0)
                    throw new ArgumentException("Total pages cannot be negative");
                return _userBookshelfDAL.UpdateReadingProgress(userId, bookId, currentPage, readingProgress, totalPages);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating reading progress: {ex.Message}", ex);
            }
        }
    }
}
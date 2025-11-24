using BookHub.DAL;

namespace BookHub.BLL
{
    public class UserBookshelfBLL
    {
        private readonly UserBookshelfDAL _userBookshelfDAL;
        private readonly BookDAL _bookDAL;

        public UserBookshelfBLL(string connectionString)
        {
            _userBookshelfDAL = new UserBookshelfDAL(connectionString);
            _bookDAL = new BookDAL(connectionString);
        }

        // Add a book to user's bookshelf
        public bool AddBookToUserBookshelf(int userId, int bookId, string status = "Want to Read", string ownershipType = "Physical")
        {
            try
            {
                // Validate inputs
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");

                // Validate status
                var validStatuses = new[] { "Want to Read", "Reading", "Read" };
                if (!validStatuses.Contains(status))
                    status = "Want to Read";

                // Check if book exists
                var book = _bookDAL.GetBookById(bookId);
                if (book == null)
                    throw new ArgumentException("Book not found");

                // Check if already in bookshelf
                if (_userBookshelfDAL.IsBookInUserBookshelf(userId, bookId))
                    return false; // Already exists

                // Add to bookshelf
                return _userBookshelfDAL.AddBookToUserBookshelf(userId, bookId, status, ownershipType);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation errors
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error adding book to bookshelf: {ex.Message}", ex);
            }
        }

        // Get user's complete bookshelf
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

        // Get books by status
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

        // Update book status in user's bookshelf
        public bool UpdateBookStatus(int userId, int bookId, string newStatus)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");

                // Validate status
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

        // Update ownership type for a book in user's bookshelf
        public bool UpdateOwnershipType(int userId, int bookId, string newOwnershipType)
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");

                // Validate ownership type
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

        // Remove book from user's bookshelf
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

        // Update book status
        public bool UpdateBookStatus(int userId, int bookId, string status, int? rating = null, string notes = "")
        {
            try
            {
                if (userId <= 0)
                    throw new ArgumentException("Invalid user ID");
                
                if (bookId <= 0)
                    throw new ArgumentException("Invalid book ID");

                // Validate status
                var validStatuses = new[] { "Want to Read", "Reading", "Read" };
                if (!validStatuses.Contains(status))
                    throw new ArgumentException("Invalid status");

                // Validate rating
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

        // Check if book is in user's bookshelf
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
                return false; // Return false on error to be safe
            }
        }

        // Get bookshelf statistics
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

        // Get genre distribution for user's books
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
    }
}
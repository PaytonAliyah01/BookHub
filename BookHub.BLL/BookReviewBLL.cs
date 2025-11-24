using BookHub.DAL;

namespace BookHub.BLL
{
    public class BookReviewBLL
    {
        private readonly BookReviewDAL _bookReviewDAL;

        public BookReviewBLL(string connectionString)
        {
            _bookReviewDAL = new BookReviewDAL(connectionString);
        }

        public bool AddReview(BookReview review)
        {
            // Validate the review
            if (review == null)
                return false;

            if (review.Rating < 1 || review.Rating > 5)
                return false;

            if (string.IsNullOrWhiteSpace(review.ReviewTitle) || review.ReviewTitle.Length > 200)
                return false;

            if (string.IsNullOrWhiteSpace(review.ReviewText))
                return false;

            try
            {
                return _bookReviewDAL.AddReview(review);
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateReview(BookReview review)
        {
            // Validate the review
            if (review == null || review.ReviewId <= 0)
                return false;

            if (review.Rating < 1 || review.Rating > 5)
                return false;

            if (string.IsNullOrWhiteSpace(review.ReviewTitle) || review.ReviewTitle.Length > 200)
                return false;

            if (string.IsNullOrWhiteSpace(review.ReviewText))
                return false;

            try
            {
                return _bookReviewDAL.UpdateReview(review);
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteReview(int reviewId, int userId)
        {
            if (reviewId <= 0 || userId <= 0)
                return false;

            try
            {
                return _bookReviewDAL.DeleteReview(reviewId, userId);
            }
            catch
            {
                return false;
            }
        }

        public List<BookReview> GetReviewsForBook(int bookId)
        {
            if (bookId <= 0)
                return new List<BookReview>();

            try
            {
                return _bookReviewDAL.GetReviewsForBook(bookId);
            }
            catch
            {
                return new List<BookReview>();
            }
        }

        public BookReview? GetUserReviewForBook(int userId, int bookId)
        {
            if (userId <= 0 || bookId <= 0)
                return null;

            try
            {
                return _bookReviewDAL.GetUserReviewForBook(userId, bookId);
            }
            catch
            {
                return null;
            }
        }

        public (double averageRating, int reviewCount) GetBookRatingStats(int bookId)
        {
            if (bookId <= 0)
                return (0.0, 0);

            try
            {
                return _bookReviewDAL.GetBookRatingStats(bookId);
            }
            catch
            {
                return (0.0, 0);
            }
        }
    }
}
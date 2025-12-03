using BookHub.DAL;
using System.Linq;
namespace BookHub.BLL
{
    public class BookReviewBLL : IBookReviewBLL
    {
        private readonly BookReviewDAL _bookReviewDAL;
        public BookReviewBLL(string connectionString)
        {
            _bookReviewDAL = new BookReviewDAL(connectionString);
        }
        public bool AddReview(BookReviewDto reviewDto)
        {
            if (reviewDto == null)
                return false;
            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
                return false;
            if (string.IsNullOrWhiteSpace(reviewDto.ReviewText))
                return false;
            try
            {
                var review = MapFromDto(reviewDto);
                return _bookReviewDAL.AddReview(review);
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateReview(BookReviewDto reviewDto)
        {
            if (reviewDto == null || reviewDto.ReviewId <= 0)
                return false;
            if (reviewDto.Rating < 1 || reviewDto.Rating > 5)
                return false;
            if (string.IsNullOrWhiteSpace(reviewDto.ReviewText))
                return false;
            try
            {
                var review = MapFromDto(reviewDto);
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
        public List<BookReviewDto> GetReviewsForBook(int bookId)
        {
            if (bookId <= 0)
                return new List<BookReviewDto>();
            try
            {
                var reviews = _bookReviewDAL.GetReviewsForBook(bookId);
                return reviews.Select(MapToDto).ToList();
            }
            catch
            {
                return new List<BookReviewDto>();
            }
        }
        public BookReviewDto? GetUserReviewForBook(int userId, int bookId)
        {
            if (userId <= 0 || bookId <= 0)
                return null;
            try
            {
                var review = _bookReviewDAL.GetUserReviewForBook(userId, bookId);
                return review != null ? MapToDto(review) : null;
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
        private BookReviewDto MapToDto(BookReview review)
        {
            return new BookReviewDto
            {
                ReviewId = review.ReviewId,
                UserId = review.UserId,
                BookId = review.BookId,
                ReviewTitle = review.ReviewTitle,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                ReviewDate = review.ReviewDate,
                LastModified = review.LastModified,
                IsRecommended = review.IsRecommended,
                Username = review.Username ?? "",
                BookTitle = review.BookTitle ?? "",
                BookAuthor = review.BookAuthor ?? "",
                BookGenre = review.BookGenre ?? "",
                UserEmail = review.UserEmail ?? ""
            };
        }
        private BookReview MapFromDto(BookReviewDto reviewDto)
        {
            return new BookReview
            {
                ReviewId = reviewDto.ReviewId,
                UserId = reviewDto.UserId,
                BookId = reviewDto.BookId,
                ReviewTitle = reviewDto.ReviewTitle,
                ReviewText = reviewDto.ReviewText,
                Rating = reviewDto.Rating,
                ReviewDate = reviewDto.ReviewDate != default ? reviewDto.ReviewDate : DateTime.Now,
                LastModified = reviewDto.LastModified,
                IsRecommended = reviewDto.IsRecommended,
                Username = reviewDto.Username,
                BookTitle = reviewDto.BookTitle,
                BookAuthor = reviewDto.BookAuthor,
                BookGenre = reviewDto.BookGenre,
                UserEmail = reviewDto.UserEmail
            };
        }
    }
}
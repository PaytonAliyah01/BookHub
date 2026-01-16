using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using BookHub.DAL.Interfaces;
namespace BookHub.DAL
{
    public class BookReviewDAL : IBookReviewDAL
    {
        private readonly string _connectionString;
        public BookReviewDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public bool AddReview(BookReview review)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        INSERT INTO BookReviews (BookId, UserId, Rating, ReviewTitle, ReviewText, ReviewDate, IsRecommended)
                        VALUES (@BookId, @UserId, @Rating, @ReviewTitle, @ReviewText, @ReviewDate, @IsRecommended)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookId", review.BookId);
                        command.Parameters.AddWithValue("@UserId", review.UserId);
                        command.Parameters.AddWithValue("@Rating", review.Rating);
                        command.Parameters.AddWithValue("@ReviewTitle", review.ReviewTitle);
                        command.Parameters.AddWithValue("@ReviewText", review.ReviewText);
                        command.Parameters.AddWithValue("@ReviewDate", review.ReviewDate);
                        command.Parameters.AddWithValue("@IsRecommended", review.IsRecommended);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool UpdateReview(BookReview review)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        UPDATE BookReviews 
                        SET Rating = @Rating, ReviewTitle = @ReviewTitle, ReviewText = @ReviewText, 
                            LastModified = @LastModified, IsRecommended = @IsRecommended
                        WHERE ReviewId = @ReviewId AND UserId = @UserId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Rating", review.Rating);
                        command.Parameters.AddWithValue("@ReviewTitle", review.ReviewTitle);
                        command.Parameters.AddWithValue("@ReviewText", review.ReviewText);
                        command.Parameters.AddWithValue("@LastModified", DateTime.Now);
                        command.Parameters.AddWithValue("@IsRecommended", review.IsRecommended);
                        command.Parameters.AddWithValue("@ReviewId", review.ReviewId);
                        command.Parameters.AddWithValue("@UserId", review.UserId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public List<BookReview> GetReviewsForBook(int bookId)
        {
            var reviews = new List<BookReview>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT r.ReviewId, r.BookId, r.UserId, r.Rating, r.ReviewTitle, r.ReviewText, 
                               r.ReviewDate, r.LastModified, r.IsRecommended,
                               b.Title as BookTitle, b.Author as BookAuthor,
                               u.Username, u.Email as UserEmail
                        FROM BookReviews r
                        JOIN Books b ON r.BookId = b.BookId
                        JOIN Users u ON r.UserId = u.UserId
                        WHERE r.BookId = @BookId
                        ORDER BY r.ReviewDate DESC";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookId", bookId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reviews.Add(new BookReview
                                {
                                    ReviewId = (int)reader["ReviewId"],
                                    BookId = (int)reader["BookId"],
                                    UserId = (int)reader["UserId"],
                                    Rating = (int)reader["Rating"],
                                    ReviewTitle = reader["ReviewTitle"]?.ToString() ?? "",
                                    ReviewText = reader["ReviewText"]?.ToString() ?? "",
                                    ReviewDate = (DateTime)reader["ReviewDate"],
                                    LastModified = reader["LastModified"] == DBNull.Value ? null : (DateTime?)reader["LastModified"],
                                    IsRecommended = (bool)reader["IsRecommended"],
                                    BookTitle = reader["BookTitle"]?.ToString() ?? "",
                                    BookAuthor = reader["BookAuthor"]?.ToString() ?? "",
                                    Username = reader["Username"]?.ToString() ?? "",
                                    UserEmail = reader["UserEmail"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return reviews;
        }
        public BookReview? GetUserReviewForBook(int userId, int bookId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT r.ReviewId, r.BookId, r.UserId, r.Rating, r.ReviewTitle, r.ReviewText, 
                               r.ReviewDate, r.LastModified, r.IsRecommended,
                               b.Title as BookTitle, b.Author as BookAuthor
                        FROM BookReviews r
                        JOIN Books b ON r.BookId = b.BookId
                        WHERE r.UserId = @UserId AND r.BookId = @BookId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@BookId", bookId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new BookReview
                                {
                                    ReviewId = (int)reader["ReviewId"],
                                    BookId = (int)reader["BookId"],
                                    UserId = (int)reader["UserId"],
                                    Rating = (int)reader["Rating"],
                                    ReviewTitle = reader["ReviewTitle"]?.ToString() ?? "",
                                    ReviewText = reader["ReviewText"]?.ToString() ?? "",
                                    ReviewDate = (DateTime)reader["ReviewDate"],
                                    LastModified = reader["LastModified"] == DBNull.Value ? null : (DateTime?)reader["LastModified"],
                                    IsRecommended = (bool)reader["IsRecommended"],
                                    BookTitle = reader["BookTitle"]?.ToString() ?? "",
                                    BookAuthor = reader["BookAuthor"]?.ToString() ?? ""
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
        public bool DeleteReview(int reviewId, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "DELETE FROM BookReviews WHERE ReviewId = @ReviewId AND UserId = @UserId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReviewId", reviewId);
                        command.Parameters.AddWithValue("@UserId", userId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public (double averageRating, int reviewCount) GetBookRatingStats(int bookId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT AVG(CAST(Rating AS FLOAT)) as AverageRating, COUNT(*) as ReviewCount
                        FROM BookReviews 
                        WHERE BookId = @BookId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@BookId", bookId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var avgRating = reader["AverageRating"] == DBNull.Value ? 0.0 : (double)reader["AverageRating"];
                                var reviewCount = (int)reader["ReviewCount"];
                                return (avgRating, reviewCount);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return (0.0, 0);
        }

        public List<BookReview> GetReviewsByBookId(int bookId)
        {
            return GetReviewsForBook(bookId);
        }

        public List<BookReview> GetReviewsByUserId(int userId)
        {
            var reviews = new List<BookReview>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT r.ReviewId, r.BookId, r.UserId, r.Rating, r.ReviewTitle, r.ReviewText, 
                               r.ReviewDate, r.LastModified, r.IsRecommended,
                               b.Title as BookTitle, b.Author as BookAuthor
                        FROM BookReviews r
                        JOIN Books b ON r.BookId = b.BookId
                        WHERE r.UserId = @UserId
                        ORDER BY r.ReviewDate DESC";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                reviews.Add(new BookReview
                                {
                                    ReviewId = (int)reader["ReviewId"],
                                    BookId = (int)reader["BookId"],
                                    UserId = (int)reader["UserId"],
                                    Rating = (int)reader["Rating"],
                                    ReviewTitle = reader["ReviewTitle"]?.ToString() ?? "",
                                    ReviewText = reader["ReviewText"]?.ToString() ?? "",
                                    ReviewDate = (DateTime)reader["ReviewDate"],
                                    LastModified = reader["LastModified"] == DBNull.Value ? null : (DateTime?)reader["LastModified"],
                                    IsRecommended = (bool)reader["IsRecommended"],
                                    BookTitle = reader["BookTitle"]?.ToString() ?? "",
                                    BookAuthor = reader["BookAuthor"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return reviews;
        }

        // Overload methods for simpler test signatures
        public int AddReview(int userId, int bookId, int rating, string reviewText, DateTime reviewDate)
        {
            var review = new BookReview
            {
                UserId = userId,
                BookId = bookId,
                Rating = rating,
                ReviewTitle = "Review",
                ReviewText = reviewText,
                ReviewDate = reviewDate,
                IsRecommended = rating >= 4
            };
            bool success = AddReview(review);
            return success ? 1 : 0;
        }

        public bool UpdateReview(int reviewId, int rating, string reviewText)
        {
            var review = new BookReview
            {
                ReviewId = reviewId,
                Rating = rating,
                ReviewTitle = "Updated Review",
                ReviewText = reviewText,
                IsRecommended = rating >= 4
            };
            return UpdateReview(review);
        }

        public bool DeleteReview(int reviewId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = "DELETE FROM BookReviews WHERE ReviewId = @ReviewId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ReviewId", reviewId);
                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
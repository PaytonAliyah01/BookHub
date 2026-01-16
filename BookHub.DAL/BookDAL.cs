using Microsoft.Data.SqlClient;
using BookHub.DAL.Interfaces;
namespace BookHub.DAL
{
    public class BookDAL : IBookDAL
    {
        private readonly string _connectionString;
        public BookDAL(string connectionString)
        {
            _connectionString = connectionString;
        }
        public List<Book> GetAllBooks()
        {
            try
            {
                var books = new List<Book>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT BookId, Title, Author, ISBN, CoverUrl, Genre, Description FROM Books", conn);
                    conn.Open();
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            BookId = (int)reader["BookId"],
                            Title = reader["Title"]?.ToString() ?? "",
                            Author = reader["Author"]?.ToString() ?? "",
                            ISBN = reader["ISBN"]?.ToString() ?? "",
                            CoverUrl = reader["CoverUrl"]?.ToString() ?? "",
                            Genre = reader["Genre"]?.ToString() ?? "",
                            Description = reader["Description"]?.ToString() ?? ""
                        });
                    }
                }
                return books;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving books: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving books: {ex.Message}", ex);
            }
        }
        public Book? GetBookById(int bookId)
        {
            try
            {
                if (bookId <= 0)
                    return null;
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT BookId, Title, Author, ISBN, CoverUrl, Genre, Description FROM Books WHERE BookId = @BookId", conn);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    conn.Open();
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return new Book
                        {
                            BookId = (int)reader["BookId"],
                            Title = reader["Title"]?.ToString() ?? "",
                            Author = reader["Author"]?.ToString() ?? "",
                            ISBN = reader["ISBN"]?.ToString() ?? "",
                            CoverUrl = reader["CoverUrl"]?.ToString() ?? "",
                            Genre = reader["Genre"]?.ToString() ?? "",
                            Description = reader["Description"]?.ToString() ?? ""
                        };
                    }
                }
                return null;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while retrieving book: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving book: {ex.Message}", ex);
            }
        }
        public int AddBook(Book book)
        {
            try
            {
                if (book == null)
                    throw new ArgumentNullException(nameof(book));
                if (string.IsNullOrWhiteSpace(book.Title))
                    throw new ArgumentException("Book title cannot be empty", nameof(book));
                if (string.IsNullOrWhiteSpace(book.Author))
                    throw new ArgumentException("Book author cannot be empty", nameof(book));
                if (!string.IsNullOrWhiteSpace(book.ISBN) && BookExistsByISBN(book.ISBN))
                    throw new InvalidOperationException("A book with this ISBN already exists");
                if (string.IsNullOrWhiteSpace(book.ISBN) && BookExistsByTitleAndAuthor(book.Title, book.Author))
                    throw new InvalidOperationException("A book with this title and author already exists");
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Books (Title, Author, ISBN, CoverUrl, Genre) 
                        VALUES (@Title, @Author, @ISBN, @CoverUrl, @Genre);
                        SELECT SCOPE_IDENTITY();", conn);
                    cmd.Parameters.AddWithValue("@Title", book.Title);
                    cmd.Parameters.AddWithValue("@Author", book.Author);
                    cmd.Parameters.AddWithValue("@ISBN", book.ISBN ?? "");
                    cmd.Parameters.AddWithValue("@CoverUrl", book.CoverUrl ?? "");
                    cmd.Parameters.AddWithValue("@Genre", book.Genre ?? "");
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new InvalidOperationException("A book with this information already exists.", ex);
                }
                throw new InvalidOperationException($"Database error occurred while adding book: {ex.Message}", ex);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while adding book: {ex.Message}", ex);
            }
        }
        private bool BookExistsByISBN(string isbn)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Books WHERE ISBN = @ISBN AND ISBN != ''", conn);
                    cmd.Parameters.AddWithValue("@ISBN", isbn);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while checking book existence: {ex.Message}", ex);
            }
        }
        private bool BookExistsByTitleAndAuthor(string title, string author)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Books WHERE LOWER(Title) = LOWER(@Title) AND LOWER(Author) = LOWER(@Author)", conn);
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Author", author);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while checking book existence: {ex.Message}", ex);
            }
        }
        public bool UpdateBookCover(int bookId, string coverUrl)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE Books SET CoverUrl = @CoverUrl WHERE BookId = @BookId", conn);
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@CoverUrl", coverUrl ?? "");
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while updating book cover: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating book cover: {ex.Message}", ex);
            }
        }
        public bool DeleteBook(int bookId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            SqlCommand deleteUserBooks = new SqlCommand("DELETE FROM UserBooks WHERE BookId = @BookId", conn, transaction);
                            deleteUserBooks.Parameters.AddWithValue("@BookId", bookId);
                            deleteUserBooks.ExecuteNonQuery();
                            SqlCommand deleteBook = new SqlCommand("DELETE FROM Books WHERE BookId = @BookId", conn, transaction);
                            deleteBook.Parameters.AddWithValue("@BookId", bookId);
                            int rowsAffected = deleteBook.ExecuteNonQuery();
                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while deleting book: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while deleting book: {ex.Message}", ex);
            }
        }
        public int AddBook(string title, string author, string isbn, string genre, string description, string coverUrl)
        {
            var book = new Book
            {
                Title = title,
                Author = author,
                ISBN = isbn,
                Genre = genre,
                Description = description,
                CoverUrl = coverUrl
            };
            return AddBook(book);
        }
        public bool UpdateBook(int bookId, string title, string author, string isbn, string genre, string description, string coverUrl)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = @"
                        UPDATE Books 
                        SET Title = @Title, Author = @Author, ISBN = @ISBN, 
                            Genre = @Genre, Description = @Description, CoverUrl = @CoverUrl
                        WHERE BookId = @BookId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookId", bookId);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Author", author);
                        cmd.Parameters.AddWithValue("@ISBN", isbn ?? "");
                        cmd.Parameters.AddWithValue("@Genre", genre ?? "");
                        cmd.Parameters.AddWithValue("@Description", description ?? "");
                        cmd.Parameters.AddWithValue("@CoverUrl", coverUrl ?? "");
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while updating book: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating book: {ex.Message}", ex);
            }
        }

        public List<Book> SearchBooks(string keyword)
        {
            try
            {
                var books = new List<Book>();
                
                if (string.IsNullOrWhiteSpace(keyword))
                    return GetAllBooks();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    SqlCommand cmd = new SqlCommand(@"
                        SELECT BookId, Title, Author, ISBN, CoverUrl, Genre, Description 
                        FROM Books 
                        WHERE Title LIKE @Keyword 
                           OR Author LIKE @Keyword 
                           OR Genre LIKE @Keyword 
                           OR Description LIKE @Keyword
                           OR ISBN LIKE @Keyword", conn);
                    
                    cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
                    
                    conn.Open();
                    using var reader = cmd.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        books.Add(new Book
                        {
                            BookId = (int)reader["BookId"],
                            Title = reader["Title"]?.ToString() ?? "",
                            Author = reader["Author"]?.ToString() ?? "",
                            ISBN = reader["ISBN"]?.ToString() ?? "",
                            CoverUrl = reader["CoverUrl"]?.ToString() ?? "",
                            Genre = reader["Genre"]?.ToString() ?? "",
                            Description = reader["Description"]?.ToString() ?? ""
                        });
                    }
                }
                
                return books;
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Database error occurred while searching books: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while searching books: {ex.Message}", ex);
            }
        }
    }
}

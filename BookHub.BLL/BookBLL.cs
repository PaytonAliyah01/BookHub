using BookHub.DAL;
using BookHub.DAL.Interfaces;
namespace BookHub.BLL
{
    public class BookBLL : IBookBLL
    {
        private readonly IBookDAL _bookDAL;
        public BookBLL(IBookDAL bookDAL)
        {
            _bookDAL = bookDAL;
        }
        public List<BookDto> GetAllBooks()
        {
            try
            {
                var books = _bookDAL.GetAllBooks();
                return books.Select(MapToDto).ToList();
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve books. Please try again later.", ex);
            }
        }
        public BookDto? GetBookById(int bookId)
        {
            try
            {
                if (bookId <= 0)
                    return null;
                var book = _bookDAL.GetBookById(bookId);
                return book != null ? MapToDto(book) : null;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve book. Please try again later.", ex);
            }
        }
        internal BookHub.DAL.Book? GetBookByIdInternal(int bookId)
        {
            try
            {
                if (bookId <= 0)
                    return null;
                return _bookDAL.GetBookById(bookId);
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to retrieve book. Please try again later.", ex);
            }
        }
        public int AddBook(BookDto bookDto)
        {
            try
            {
                if (bookDto == null)
                    throw new ArgumentException("Book cannot be null");
                if (string.IsNullOrWhiteSpace(bookDto.Title?.Trim()))
                    throw new ArgumentException("Book title is required");
                if (string.IsNullOrWhiteSpace(bookDto.Author?.Trim()))
                    throw new ArgumentException("Book author is required");
                if (bookDto.Title.Length > 255)
                    throw new ArgumentException("Book title cannot exceed 255 characters");
                if (bookDto.Author.Length > 255)
                    throw new ArgumentException("Book author cannot exceed 255 characters");
                var dalBook = MapFromDto(bookDto);
                dalBook.Title = dalBook.Title.Trim();
                dalBook.Author = dalBook.Author.Trim();
                dalBook.ISBN = dalBook.ISBN?.Trim() ?? "";
                dalBook.Genre = dalBook.Genre?.Trim() ?? "";
                dalBook.CoverUrl = dalBook.CoverUrl?.Trim() ?? "";
                return _bookDAL.AddBook(dalBook);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException("Unable to add book. Please try again later.", ex);
            }
        }
        private BookDto MapToDto(BookHub.DAL.Book book)
        {
            return new BookDto
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                CoverUrl = book.CoverUrl,
                CoverImagePath = book.CoverUrl,
                Genre = book.Genre,
                Description = book.Description,
                CreatedDate = book.CreatedDate,
                PublicationDate = book.CreatedDate,
                GenreId = 0,
                TotalPages = 0
            };
        }
        private BookHub.DAL.Book MapFromDto(BookDto bookDto)
        {
            return new BookHub.DAL.Book
            {
                BookId = bookDto.BookId,
                Title = bookDto.Title,
                Author = bookDto.Author,
                ISBN = bookDto.ISBN,
                CoverUrl = bookDto.CoverUrl ?? bookDto.CoverImagePath ?? "",
                Genre = bookDto.Genre,
                Description = bookDto.Description,
                CreatedDate = bookDto.CreatedDate != default ? bookDto.CreatedDate : DateTime.Now
            };
        }
    }
}

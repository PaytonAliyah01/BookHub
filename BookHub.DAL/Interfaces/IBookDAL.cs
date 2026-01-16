namespace BookHub.DAL.Interfaces
{
    public interface IBookDAL
    {
        List<Book> GetAllBooks();
        Book? GetBookById(int bookId);
        int AddBook(Book book);
        int AddBook(string title, string author, string isbn, string genre, string description, string coverUrl);
        bool UpdateBookCover(int bookId, string coverUrl);
        bool UpdateBook(int bookId, string title, string author, string isbn, string genre, string description, string coverUrl);
        bool DeleteBook(int bookId);
        
        // New methods for search functionality
        List<Book> SearchBooks(string keyword);
    }
}
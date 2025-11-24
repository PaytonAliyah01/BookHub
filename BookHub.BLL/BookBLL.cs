using BookHub.DAL;

namespace BookHub.BLL
{
    public class BookBLL
    {
        private readonly BookDAL _bookDAL;
        private readonly CoverDownloadService _coverService;

        public BookBLL(string connectionString)
        {
            _bookDAL = new BookDAL(connectionString);
            _coverService = new CoverDownloadService();
        }

        // Business logic: Get all books with any business rules applied
        public List<BookHub.DAL.Book> GetAllBooks()
        {
            try
            {
                return _bookDAL.GetAllBooks();
            }
            catch (InvalidOperationException ex)
            {
                // Handle DAL exceptions and convert to business exceptions
                throw new ApplicationException("Unable to retrieve books. Please try again later.", ex);
            }
        }

        // Business logic: Get a specific book with validation
        public BookHub.DAL.Book? GetBookById(int bookId)
        {
            try
            {
                if (bookId <= 0)
                    return null;

                return _bookDAL.GetBookById(bookId);
            }
            catch (InvalidOperationException ex)
            {
                // Handle DAL exceptions
                throw new ApplicationException("Unable to retrieve book. Please try again later.", ex);
            }
        }

        // Business logic: Add a new book with validation
        public int AddBook(BookHub.DAL.Book book)
        {
            try
            {
                if (book == null)
                    throw new ArgumentException("Book cannot be null");

                // Business logic validations
                if (string.IsNullOrWhiteSpace(book.Title?.Trim()))
                    throw new ArgumentException("Book title is required");

                if (string.IsNullOrWhiteSpace(book.Author?.Trim()))
                    throw new ArgumentException("Book author is required");

                if (book.Title.Length > 255)
                    throw new ArgumentException("Book title cannot exceed 255 characters");

                if (book.Author.Length > 255)
                    throw new ArgumentException("Book author cannot exceed 255 characters");

                // Clean up the data
                book.Title = book.Title.Trim();
                book.Author = book.Author.Trim();
                book.ISBN = book.ISBN?.Trim() ?? "";
                book.Genre = book.Genre?.Trim() ?? "";
                book.CoverUrl = book.CoverUrl?.Trim() ?? "";

                return _bookDAL.AddBook(book);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException ex)
            {
                // Handle DAL exceptions
                throw new ApplicationException("Unable to add book. Please try again later.", ex);
            }
        }

        // Download covers for all books that don't have local covers
        public async Task<string> DownloadMissingCoversAsync()
        {
            try
            {
                var books = _bookDAL.GetAllBooks();
                var downloadTasks = new List<Task<string?>>();
                var results = new List<string>();

                Console.WriteLine($"🔍 Checking covers for {books.Count} books...");

                foreach (var book in books)
                {
                    // Check if book already has a local cover
                    var localCoverPath = Path.Combine("wwwroot", "images", "covers", $"book_{book.BookId}.jpg");
                    
                    if (!File.Exists(localCoverPath))
                    {
                        var task = _coverService.DownloadAndSaveCoverAsync(book.BookId, book.Title, book.Author, book.ISBN);
                        downloadTasks.Add(task);
                    }
                    else
                    {
                        results.Add($"✅ {book.Title} - Cover already exists");
                    }
                }

                Console.WriteLine($"📥 Downloading {downloadTasks.Count} missing covers...");
                
                // Process downloads in batches to avoid overwhelming servers
                var batchSize = 5;
                for (int i = 0; i < downloadTasks.Count; i += batchSize)
                {
                    var batch = downloadTasks.Skip(i).Take(batchSize);
                    var batchResults = await Task.WhenAll(batch);
                    
                    foreach (var result in batchResults)
                    {
                        if (!string.IsNullOrEmpty(result))
                        {
                            results.Add($"✅ Downloaded: {result}");
                        }
                    }
                    
                    // Small delay between batches
                    await Task.Delay(1000);
                }

                var summary = $"📊 Cover Download Summary:\n" +
                             $"   Total Books: {books.Count}\n" +
                             $"   Downloads Attempted: {downloadTasks.Count}\n" +
                             $"   Successful Downloads: {results.Count(r => r.Contains("Downloaded"))}\n" +
                             $"   Already Had Covers: {results.Count(r => r.Contains("already exists"))}";

                Console.WriteLine(summary);
                return summary;
            }
            catch (Exception ex)
            {
                var errorMsg = $"❌ Error downloading covers: {ex.Message}";
                Console.WriteLine(errorMsg);
                return errorMsg;
            }
        }

        // Future business logic methods can be added here:
        // - SearchBooks(string searchTerm)
        // - GetBooksByCategory(string category)
        // - GetFeaturedBooks()
        // - etc.
    }
}

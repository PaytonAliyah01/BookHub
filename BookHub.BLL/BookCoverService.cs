using System.Text.Json;

namespace BookHub.BLL
{
    public class BookCoverService
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;

        public BookCoverService(string connectionString)
        {
            _connectionString = connectionString;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookHub/1.0");
        }

        public async Task<string?> GetBookCoverUrlAsync(string title, string author, string isbn)
        {
            // Priority order for cover sources
            var coverUrl = await TryOpenLibraryByISBN(isbn);
            if (!string.IsNullOrEmpty(coverUrl)) return coverUrl;

            coverUrl = await TryOpenLibraryByTitle(title, author);
            if (!string.IsNullOrEmpty(coverUrl)) return coverUrl;

            coverUrl = await TryGoogleBooksAPI(title, author, isbn);
            if (!string.IsNullOrEmpty(coverUrl)) return coverUrl;

            // Return null to fall back to generated covers
            return null;
        }

        private async Task<string?> TryOpenLibraryByISBN(string isbn)
        {
            if (string.IsNullOrEmpty(isbn) || isbn.Length < 10) return null;

            try
            {
                var cleanISBN = isbn.Replace("-", "").Replace(" ", "");
                var testUrl = $"https://covers.openlibrary.org/b/isbn/{cleanISBN}-L.jpg";
                
                // Test if the cover exists
                var response = await _httpClient.GetAsync(testUrl);
                if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 1000)
                {
                    return testUrl;
                }
            }
            catch { }
            
            return null;
        }

        private async Task<string?> TryOpenLibraryByTitle(string title, string author)
        {
            if (string.IsNullOrEmpty(title)) return null;

            try
            {
                // Search for the book first
                var searchQuery = Uri.EscapeDataString($"{title} {author}".Trim());
                var searchUrl = $"https://openlibrary.org/search.json?q={searchQuery}&limit=1";
                
                var response = await _httpClient.GetAsync(searchUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var searchResult = JsonSerializer.Deserialize<OpenLibrarySearchResponse>(content);
                    
                    if (searchResult?.docs?.Length > 0 && searchResult.docs[0].cover_i.HasValue)
                    {
                        return $"https://covers.openlibrary.org/b/id/{searchResult.docs[0].cover_i}-L.jpg";
                    }
                }
            }
            catch { }

            return null;
        }

        private async Task<string?> TryGoogleBooksAPI(string title, string author, string isbn)
        {
            try
            {
                string query;
                if (!string.IsNullOrEmpty(isbn))
                {
                    query = $"isbn:{isbn.Replace("-", "")}";
                }
                else
                {
                    query = Uri.EscapeDataString($"{title} {author}".Trim());
                }

                var apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={query}&maxResults=1";
                var response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GoogleBooksResponse>(content);
                    
                    if (result?.items?.Length > 0)
                    {
                        var book = result.items[0];
                        var imageLinks = book.volumeInfo?.imageLinks;
                        
                        // Prefer large images, fall back to smaller ones
                        if (!string.IsNullOrEmpty(imageLinks?.large))
                            return imageLinks.large.Replace("http:", "https:");
                        if (!string.IsNullOrEmpty(imageLinks?.medium))
                            return imageLinks.medium.Replace("http:", "https:");
                        if (!string.IsNullOrEmpty(imageLinks?.small))
                            return imageLinks.small.Replace("http:", "https:");
                        if (!string.IsNullOrEmpty(imageLinks?.thumbnail))
                            return imageLinks.thumbnail.Replace("http:", "https:");
                    }
                }
            }
            catch { }

            return null;
        }

        public async Task UpdateAllBookCoversAsync()
        {
            var bookDAL = new BookHub.DAL.BookDAL(_connectionString);
            var books = bookDAL.GetAllBooks();

            var tasks = books.Select(async book =>
            {
                try
                {
                    var coverUrl = await GetBookCoverUrlAsync(book.Title, book.Author, book.ISBN);
                    if (!string.IsNullOrEmpty(coverUrl))
                    {
                        // Update the book's cover URL in database
                        bookDAL.UpdateBookCover(book.BookId, coverUrl);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other books
                    Console.WriteLine($"Error updating cover for {book.Title}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    // DTOs for API responses
    public class OpenLibrarySearchResponse
    {
        public OpenLibraryDoc[]? docs { get; set; }
    }

    public class OpenLibraryDoc
    {
        public int? cover_i { get; set; }
        public string? title { get; set; }
        public string[]? author_name { get; set; }
    }

    public class GoogleBooksResponse
    {
        public GoogleBooksItem[]? items { get; set; }
    }

    public class GoogleBooksItem
    {
        public GoogleBooksVolumeInfo? volumeInfo { get; set; }
    }

    public class GoogleBooksVolumeInfo
    {
        public GoogleBooksImageLinks? imageLinks { get; set; }
    }

    public class GoogleBooksImageLinks
    {
        public string? thumbnail { get; set; }
        public string? small { get; set; }
        public string? medium { get; set; }
        public string? large { get; set; }
    }
}
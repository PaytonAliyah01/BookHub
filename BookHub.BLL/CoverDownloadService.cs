using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BookHub.BLL
{
    public class CoverDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _coversDirectory;

        public CoverDownloadService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            
            // Set up covers directory in wwwroot
            _coversDirectory = Path.Combine("wwwroot", "images", "covers");
            Directory.CreateDirectory(_coversDirectory);
        }

        public async Task<string?> DownloadAndSaveCoverAsync(int bookId, string title, string author, string isbn)
        {
            try
            {
                // Check if cover already exists locally
                var localCoverPath = GetLocalCoverPath(bookId);
                var webPath = GetWebCoverPath(bookId);
                
                if (File.Exists(localCoverPath))
                {
                    return webPath; // Return web-accessible path
                }

                // Try to download from multiple sources
                var coverSources = GetCoverSources(title, author, isbn);
                
                foreach (var coverUrl in coverSources)
                {
                    try
                    {
                        var imageBytes = await DownloadImageAsync(coverUrl);
                        if (imageBytes != null && imageBytes.Length > 1000) // Ensure it's a real image
                        {
                            await File.WriteAllBytesAsync(localCoverPath, imageBytes);
                            Console.WriteLine($"✅ Downloaded cover for '{title}' from {coverUrl}");
                            return webPath;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to download from {coverUrl}: {ex.Message}");
                    }
                }

                Console.WriteLine($"🎨 No cover found for '{title}', will use generated cover");
                return null; // Fall back to generated cover
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error downloading cover for '{title}': {ex.Message}");
                return null;
            }
        }

        private List<string> GetCoverSources(string title, string author, string isbn)
        {
            var sources = new List<string>();
            
            // Clean inputs
            var cleanTitle = title?.Replace(" ", "+").Replace("'", "").Replace("\"", "") ?? "";
            var cleanISBN = isbn?.Replace("-", "").Replace(" ", "") ?? "";

            // ISBN-based sources (most reliable)
            if (!string.IsNullOrEmpty(cleanISBN))
            {
                sources.Add($"https://covers.openlibrary.org/b/isbn/{cleanISBN}-L.jpg");
                sources.Add($"https://covers.openlibrary.org/b/isbn/{cleanISBN}-M.jpg");
                
                // Try different ISBN formats
                if (cleanISBN.Length == 13)
                {
                    sources.Add($"https://covers.openlibrary.org/b/isbn/{cleanISBN.Substring(3)}-L.jpg");
                }
            }

            // Title-based sources
            if (!string.IsNullOrEmpty(cleanTitle))
            {
                sources.Add($"https://covers.openlibrary.org/b/title/{cleanTitle}-L.jpg");
                sources.Add($"https://covers.openlibrary.org/b/title/{cleanTitle}-M.jpg");
            }

            // Google Books API
            if (!string.IsNullOrEmpty(cleanISBN))
            {
                sources.Add($"https://books.google.com/books/content?id={cleanISBN}&printsec=frontcover&img=1&zoom=1&source=gbs_api");
            }

            return sources;
        }

        private async Task<byte[]?> DownloadImageAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    
                    // Check if it's a valid image (basic validation)
                    if (bytes.Length > 1000 && IsValidImageBytes(bytes))
                    {
                        return bytes;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore individual failures, we have fallbacks
            }
            
            return null;
        }

        private bool IsValidImageBytes(byte[] bytes)
        {
            // Check for common image format headers
            if (bytes.Length < 4) return false;
            
            // JPEG
            if (bytes[0] == 0xFF && bytes[1] == 0xD8) return true;
            
            // PNG
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;
            
            // GIF
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46) return true;
            
            return false;
        }

        private string GetLocalCoverPath(int bookId)
        {
            return Path.Combine(_coversDirectory, $"book_{bookId}.jpg");
        }

        private string GetWebCoverPath(int bookId)
        {
            return $"/images/covers/book_{bookId}.jpg";
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
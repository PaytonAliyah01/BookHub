using System.ComponentModel.DataAnnotations;

namespace BookHub.DAL
{
    public class PostAttachment
    {
        public int AttachmentId { get; set; }
        public int? PostId { get; set; }
        public int? ReplyId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        public long FileSize { get; set; }
        public DateTime UploadedDate { get; set; } = DateTime.Now;
        
        // Navigation properties
        public DiscussionPost? DiscussionPost { get; set; }
        public DiscussionReply? DiscussionReply { get; set; }
        
        // Helper properties
        public bool IsImage => FileType.StartsWith("image/");
        public bool IsGif => FileType == "image/gif";
        public string FileExtension => Path.GetExtension(FileName).ToLowerInvariant();
        public string FileSizeFormatted => FormatFileSize(FileSize);
        
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }
}
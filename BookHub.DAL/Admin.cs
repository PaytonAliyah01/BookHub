namespace BookHub.DAL
{
    public class Admin
    {
        public int AdminId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
namespace BookHub.DAL
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = "default.png";
        public bool IsRestricted { get; set; } = false;
    }
}
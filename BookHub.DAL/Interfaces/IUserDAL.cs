namespace BookHub.DAL.Interfaces
{
    public interface IUserDAL
    {
        bool UserExists(string email);
        void RegisterUser(string name, string username, string email, string hashedPassword, string salt, DateTime? dateOfBirth, string? gender);
        User? GetUserWithCredentials(string email);
        (string? passwordHash, string? salt) GetUserPasswordData(string email);
        void UpdatePassword(string email, string newPasswordHash, string newSalt);
        int? GetUserIdByEmail(string email);
        void UpdateProfile(string email, string username, string name, string bio, string profileImage, string? location = null, string? favoriteGenres = null, string? favoriteAuthors = null, string? preferredFormat = null, string? favoriteQuote = null);
    }
}
namespace BookHub.DAL.Interfaces
{
    public interface IUserDAL
    {
        bool UserExists(string email);
        void RegisterUser(string name, string email, string hashedPassword, string salt);
        User? GetUserWithCredentials(string email);
        (string? passwordHash, string? salt) GetUserPasswordData(string email);
        void UpdatePassword(string email, string newPasswordHash, string newSalt);
        int? GetUserIdByEmail(string email);
        void UpdateProfile(string email, string name, string bio, string profileImage);
    }
}
namespace BookHub.DAL.Interfaces
{
    public interface IAdminDAL
    {
        Admin? ValidateAdmin(string username, string password);
        List<Admin> GetAllAdmins();
        Dictionary<string, object> GetSystemStats();
        List<User> GetAllUsers();
        bool DeleteUser(int userId);
        bool RestrictUser(int userId, bool isRestricted);
        List<dynamic> GetAllBookClubs();
        bool DeleteBookClub(int clubId);
        List<Book> GetUserBooks(int userId);
        
        // New method for admin user management
        User? GetUserById(int userId);
    }
}
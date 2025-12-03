using BookHub.DAL;
using BookHub.DAL.Interfaces;
namespace BookHub.BLL
{
    public class AdminBLL
    {
        private readonly IAdminDAL _adminDAL;
        private readonly IBookDAL _bookDAL;
        public AdminBLL(IAdminDAL adminDAL, IBookDAL bookDAL)
        {
            _adminDAL = adminDAL;
            _bookDAL = bookDAL;
        }
        public Admin? ValidateAdmin(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return null;
                return _adminDAL.ValidateAdmin(username, password);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error validating admin: {ex.Message}", ex);
            }
        }
        public List<Admin> GetAllAdmins()
        {
            try
            {
                return _adminDAL.GetAllAdmins();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving admins: {ex.Message}", ex);
            }
        }
        public Dictionary<string, object> GetSystemStats()
        {
            try
            {
                return _adminDAL.GetSystemStats();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving system stats: {ex.Message}", ex);
            }
        }
        public List<User> GetAllUsers()
        {
            try
            {
                return _adminDAL.GetAllUsers();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving users: {ex.Message}", ex);
            }
        }
        public List<Book> GetAllBooks()
        {
            try
            {
                return _bookDAL.GetAllBooks();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving books: {ex.Message}", ex);
            }
        }
        public bool DeleteBook(int bookId)
        {
            try
            {
                return _bookDAL.DeleteBook(bookId);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting book: {ex.Message}", ex);
            }
        }
        public bool AddBook(string title, string author, string isbn, string genre, string description, string coverUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                    return false;
                return _bookDAL.AddBook(title, author, isbn ?? "", genre ?? "", description ?? "", coverUrl ?? "") > 0;
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateBook(int bookId, string title, string author, string isbn, string genre, string description, string coverUrl)
        {
            try
            {
                if (bookId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
                    return false;
                return _bookDAL.UpdateBook(bookId, title, author, isbn ?? "", genre ?? "", description ?? "", coverUrl ?? "");
            }
            catch
            {
                return false;
            }
        }
        public bool DeleteUser(int userId)
        {
            try
            {
                if (userId <= 0)
                    return false;
                return _adminDAL.DeleteUser(userId);
            }
            catch
            {
                return false;
            }
        }
        public bool RestrictUser(int userId, bool isRestricted)
        {
            try
            {
                if (userId <= 0)
                    return false;
                return _adminDAL.RestrictUser(userId, isRestricted);
            }
            catch
            {
                return false;
            }
        }
        public List<dynamic> GetAllBookClubs()
        {
            try
            {
                return _adminDAL.GetAllBookClubs();
            }
            catch
            {
                return new List<dynamic>();
            }
        }
        public bool DeleteBookClub(int clubId)
        {
            try
            {
                if (clubId <= 0)
                    return false;
                return _adminDAL.DeleteBookClub(clubId);
            }
            catch
            {
                return false;
            }
        }
        public List<Book> GetUserBooks(int userId)
        {
            try
            {
                if (userId <= 0)
                    return new List<Book>();
                return _adminDAL.GetUserBooks(userId);
            }
            catch
            {
                return new List<Book>();
            }
        }
    }
}
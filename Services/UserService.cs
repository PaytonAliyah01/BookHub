using BookHub.Data;
using BookHub.Models;
using BookHub.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace BookHub.Services
{
    public class UserService
    {
        private readonly BookHubDbContext _context;

        public UserService(BookHubDbContext context)
        {
            _context = context;
        }

        // Register a new user
        public async Task<bool> RegisterAsync(string name, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return false; // Email already exists

            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(password, salt);

            var user = new User
            {
                Name = name,
                Email = email,
                Salt = salt,
                PasswordHash = hash
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Authenticate user
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            string hash = PasswordHelper.HashPassword(password, user.Salt);
            return hash == user.PasswordHash ? user : null;
        }

        // Get user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // Update user (name/email/password if provided)
        public async Task UpdateUserAsync(User user, string? newPassword = null)
        {
            if (!string.IsNullOrEmpty(newPassword))
            {
                string salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.HashPassword(newPassword, salt);
                user.Salt = salt;
                user.PasswordHash = hash;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Update only name/email and optionally password
        public async Task UpdateProfileAsync(int userId, string name, string email, string? newPassword = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return;

            user.Name = name;
            user.Email = email;

            if (!string.IsNullOrEmpty(newPassword))
            {
                string salt = PasswordHelper.GenerateSalt();
                string hash = PasswordHelper.HashPassword(newPassword, salt);
                user.Salt = salt;
                user.PasswordHash = hash;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        // Optional: update only password
        public async Task UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return;

            string salt = PasswordHelper.GenerateSalt();
            string hash = PasswordHelper.HashPassword(newPassword, salt);
            user.Salt = salt;
            user.PasswordHash = hash;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

using BookHub.Data;
using BookHub.Models;
using BookHub.Helpers;
using Microsoft.EntityFrameworkCore;

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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Login
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            string hash = PasswordHelper.HashPassword(password, user.Salt);
            if (hash == user.PasswordHash)
                return user;

            return null;
        }
    }
}

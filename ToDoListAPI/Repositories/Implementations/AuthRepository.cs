using ToDoListAPI.Data;
using ToDoListAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ToDoListAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ToDoListContext _context;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(ToDoListContext context, ILogger<AuthRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> AddUserAsync(User user)
        {
            if (user == null)
            {
                _logger.LogWarning("Attempted to add a null user.");
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred while adding user with email {UserEmail}.", user.Email);
                throw new Exception("Database error occurred while adding the user.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding user with email {UserEmail}.", user.Email);
                throw new Exception("An unexpected error occurred while adding the user.", ex);
            }
        }

        public async Task<bool> IsEmailUnique(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Attempted to check email uniqueness with a null or empty email.");
                throw new ArgumentException("Email cannot be null or empty.", nameof(userEmail));
            }

            try
            {
                return !await _context.Users.AnyAsync(u => u.Email == userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email {UserEmail} is unique.", userEmail);
                throw new Exception("Error checking email uniqueness.", ex);
            }
        }

        public async Task<User> GetUserByEmailAsync(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Attempted to fetch user with a null or empty email.");
                throw new ArgumentException("Email cannot be null or empty.", nameof(userEmail));
            }

            try
            {
                return await _context.Users.SingleOrDefaultAsync(u => u.Email == userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with email {UserEmail}.", userEmail);
                throw new Exception("Error fetching user by email.", ex);
            }
        }
    }
}



    

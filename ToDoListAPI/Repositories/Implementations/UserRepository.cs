using ToDoListAPI.Data;
using ToDoListAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ToDoListAPI.Repositories;

public class UserRepository : IUserRepository {
    private readonly ToDoListContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ToDoListContext context, ILogger<UserRepository> logger) 
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

    public async Task<User> GetUserByIdAsync(int userId)
    {
        try
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with Id {userId}.", userId);
            throw new Exception("Error fetching user by Id.", ex);
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
            _logger.LogError(ex, "Error fetching user with email {userEmail}.", userEmail);
            throw new Exception("Error fetching user by email.", ex);
        }
    }

}
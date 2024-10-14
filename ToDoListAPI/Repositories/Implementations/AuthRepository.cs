using ToDoListAPI.Data;
using ToDoListAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ToDoListAPI.Repositories;

public class AuthRepository : IAuthRepository {
    private readonly ToDoListContext _context;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(ToDoListContext context, ILogger<AuthRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User> AddUserAsync(User user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Error adding user to the database.");
            throw new Exception("Error adding user to the database.", dbEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            throw new Exception("An unexpected error occurred.", ex);
        }
    }

    public async Task<bool> ValidateUserByEmailAsync(string userEmail)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            _logger.LogWarning("Attempted to validate user with an empty or null email.");
            throw new ArgumentException("Email cannot be null or empty.", nameof(userEmail));
        }

        try
        {
            return await _context.Users.AnyAsync(u => u.Email == userEmail);
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error occurred while validating the user with email: {Email}", userEmail);
            throw new Exception("Database error while validating the user.", dbEx);
        }
        catch (InvalidOperationException invalidOpEx)
        {
            _logger.LogError(invalidOpEx, "Invalid operation while validating the user with email: {Email}", userEmail);
            throw new Exception("Invalid operation occurred while validating the user.", invalidOpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while validating the user with email: {Email}", userEmail);
            throw new Exception("An unexpected error occurred while validating the user.", ex);
        }
    }

}


    

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
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> IsEmailUnique(string userEmail)
    {
        return !await _context.Users.AnyAsync(u => u.Email == userEmail);
    }

    public async Task<User> GetUserByEmailAsync (string userEmail){

        return await _context.Users.SingleOrDefaultAsync(u => u.Email == userEmail);
    }

}


    

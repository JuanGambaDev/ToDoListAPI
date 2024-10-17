using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public interface IUserRepository {
    Task<User> AddUserAsync(User user);
    Task<User> GetUserByIdAsync(int userId);
    Task<User> GetUserByEmailAsync (string userEmail);
}
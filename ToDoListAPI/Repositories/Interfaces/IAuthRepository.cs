using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public interface IAuthRepository {
    Task<User> AddUserAsync (User user);
    Task<bool> IsEmailUnique(string userEmail);
    Task<User> GetUserByEmailAsync (string userEmail);
}
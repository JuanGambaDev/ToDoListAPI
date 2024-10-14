using ToDoListAPI.Models;

namespace ToDoListAPI.Repositories;

public interface IAuthRepository {
    Task<User> AddUserAsync (User user);
    Task<bool> ValidateUserByEmailAsync (string userEmail);
}
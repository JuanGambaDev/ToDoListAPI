using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Repositories;

namespace ToDoListAPI.Services;

public interface IAuthService {

    Task<string> RegisterNewUser (UserRegisterRequest newUser);

    Task<string> AutenticateAsync (UserLoginRequest userCredentials);

    //string GenerateJwtToken(User user);

    //bool VerifyPassword(string password, string hashedPassword);

    //string HashPassword(string password);
}


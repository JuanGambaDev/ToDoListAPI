using ToDoListAPI.DTOs;
using ToDoListAPI.Models;
using ToDoListAPI.Repositories;

namespace ToDoListAPI.Services;

public interface IAuthService {

    Task<string> RegisterNewUser (UserRegisterRequest newUser);
    Task<(string accessToken, string refreshToken)> AutenticateAsync (UserLoginRequest userCredentials);
    Task RevokeRefreshToken (string token);
    Task<string> RefreshTokenAsync(string token);

}


using ToDoListAPI.Models;
using ToDoListAPI.DTOs;
using ToDoListAPI.Repositories;
using BCrypt.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ToDoListAPI.Services;

public class AuthService : IAuthService {
    private readonly IAuthRepository _authRepository;
    public AuthService(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<string> RegisterNewUser(UserRegisterRequest newUser)
    {
        try
        {
            var userExists = await _authRepository.ValidateUserByEmailAsync(newUser.Email);
            if (userExists == true)
            {
                throw new Exception("El correo electrónico ya está en uso.");
            }

            var user = new User 
            {
                Name = newUser.Name,
                Email = newUser.Email,
                PasswordHash = newUser.Password
            };

            var createdUser = await _authRepository.AddUserAsync(user);
            return GenerateJwtToken(createdUser);

        }
        catch (Exception ex)
        {
            throw new Exception("Error al registrar el nuevo usuario.", ex);
        }
    }

    private string GenerateJwtToken(User user)
    {
        // Configuración para la creación del token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_secret_key_here"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "your_issuer",
            audience: "your_audience",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}
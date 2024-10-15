using ToDoListAPI.Models;
using ToDoListAPI.DTOs;
using ToDoListAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;


namespace ToDoListAPI.Services;

public class AuthService : IAuthService {
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository authRepository,IConfiguration configuration, ILogger<AuthService> logger) 
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> RegisterNewUser(UserRegisterRequest newUser)
    {


            if (!await _authRepository.IsEmailUnique(newUser.Email))
            {
                throw new Exception("Email already in use.");
            }

            var user = new User 
            {
                Name = newUser.Name,
                Email = newUser.Email,
                PasswordHash = newUser.Password
            };

            await _authRepository.AddUserAsync(user);

            return GenerateJwtToken(user);

       
    }

    public async Task<string> AutenticateAsync (UserLoginRequest userCredentials)
    {
        var user = await _authRepository.GetUserByEmailAsync(userCredentials.Email);
        if (user == null || !VerifyPassword(userCredentials.Password, user.PasswordHash))
            throw new Exception("Invalid email or password.");

        return GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        if (user == null)
        throw new ArgumentNullException(nameof(user), "User cannot be null.");
    
        if (string.IsNullOrWhiteSpace(user.Name))
            throw new ArgumentNullException(nameof(user.Name), "Username cannot be null or empty.");
        
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentNullException(nameof(user.Email), "Email cannot be null or empty.");

        // Configuración para la creación del token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, $"{user.UserId}"),
            new Claim(JwtRegisteredClaimNames.Sub, user.Name),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"])),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

}
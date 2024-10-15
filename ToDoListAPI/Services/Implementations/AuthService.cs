using ToDoListAPI.Models;
using ToDoListAPI.DTOs;
using ToDoListAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;


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

            if (createdUser == null)
            {
                throw new Exception("Error al crear el usuario en la base de datos no devolvio usuario.");
            }

            return GenerateJwtToken(createdUser);

        }
        catch (Exception ex)
        {
            throw new Exception("Error al registrar el nuevo usuario.", ex);
        }
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


}
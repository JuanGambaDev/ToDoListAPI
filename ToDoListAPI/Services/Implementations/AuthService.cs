using ToDoListAPI.Models;
using ToDoListAPI.DTOs;
using ToDoListAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace ToDoListAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository authRepository, IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> RegisterNewUser(UserRegisterRequest newUser)
        {
            if (newUser == null)
            {
                _logger.LogWarning("Attempted to register a null user.");
                throw new ArgumentNullException(nameof(newUser), "User cannot be null.");
            }

            if (!await _authRepository.IsEmailUniqueAsync(newUser.Email))
            {
                _logger.LogWarning("Email already in use: {Email}.", newUser.Email);
                throw new Exception("Email already in use.");
            }

            newUser.Password = HashPassword(newUser.Password);

            var user = new User
            {
                Name = newUser.Name,
                Email = newUser.Email,
                PasswordHash = newUser.Password
            };

            await _userRepository.AddUserAsync(user);
            _logger.LogInformation("New user registered: {Email}.", user.Email);

            return GenerateAccessToken(user);
        }

        public async Task<(string accessToken, string refreshToken)> AutenticateAsync (UserLoginRequest userCredentials)
        {
            if (userCredentials == null)
            {
                _logger.LogWarning("Attempted to authenticate a null user.");
                throw new ArgumentNullException(nameof(userCredentials), "User credentials cannot be null.");
            }

            var user = await _userRepository.GetUserByEmailAsync(userCredentials.Email);
            if (user == null || !VerifyPassword(userCredentials.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}.", userCredentials.Email);
                throw new Exception("Invalid email or password.");
            }

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            _authRepository.SaveRefreshTokenAsync(new RefreshToken {
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Revoked = false,
                UserId = user.UserId
            });

            _logger.LogInformation("User authenticated: {Email}.", user.Email);
            return (accessToken, refreshToken);
        }

        public async Task<string> RefreshTokenAsync(string token)
        {
            var storedToken = await _authRepository.GetRefreshTokenAsync(token);
            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow || storedToken.Revoked)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Optionally revoke old token
            _authRepository.RevokeRefreshTokenAsync(token);

            // Generate new tokens
            var user = await _userRepository.GetUserByIdAsync(storedToken.UserId);
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Save new refresh token
            _authRepository.SaveRefreshTokenAsync(new RefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(30),
                Revoked = false
            });

            return newAccessToken; // Return new access token
        }

        public async Task RevokeRefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            await _authRepository.RevokeRefreshTokenAsync(token);
        }


        private string GenerateAccessToken(User user)
        {
            if (user == null)
            {
                _logger.LogWarning("Attempted to generate a token for a null user.");
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(user.Name))
                throw new ArgumentNullException(nameof(user.Name), "Username cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentNullException(nameof(user.Email), "Email cannot be null or empty.");

             var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim("email", user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
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

        private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Attempted to hash a null or empty password.");
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Attempted to verify a null or empty password.");
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));
            }

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

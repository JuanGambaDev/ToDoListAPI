using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoListAPI.DTOs;
using ToDoListAPI.Services;
using System.Threading.Tasks;

namespace ToDoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="newUser">The registration details of the new user.</param>
        /// <returns>A response containing the JWT token if registration is successful.</returns>
        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> RegisterNewUser(UserRegisterRequest newUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authService.RegisterNewUser(newUser);
                return CreatedAtAction(nameof(RegisterNewUser), new { }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="userCredentials">The login credentials of the user.</param>
        /// <returns>A response containing the JWT token if authentication is successful.</returns>
        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login(UserLoginRequest userCredentials)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (accessToken, refreshToken) = await _authService.AutenticateAsync(userCredentials);
                return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Login failed: {Message}", ex.Message);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("/refresh-token")]
        public IActionResult RefreshToken(string refreshToken)
        {
            try
            {
                var accessToken = _authService.RefreshTokenAsync(refreshToken);
                return Ok(new { AccessToken = accessToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Refresh token failed: {Message}", ex.Message);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during refresh token");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpPost("/logout")]
        public IActionResult Logout([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh token is required.");
            }

            try
            {
                _authService.RevokeRefreshToken(refreshToken);
                return Ok(new { Message = "Successfully logged out." });
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(500, "Internal server error. " + ex.Message);
            }
        }

    }
}


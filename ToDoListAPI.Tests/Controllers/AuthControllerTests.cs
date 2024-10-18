using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoListAPI.Controllers;
using ToDoListAPI.DTOs;
using ToDoListAPI.Services;
using Xunit;

namespace ToDoListAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task RegisterNewUser_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var newUser = new UserRegisterRequest { /* inicializar propiedades válidas */ };
            var expectedResponse = "jwt_token";
            _mockAuthService.Setup(service => service.RegisterNewUser(newUser))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RegisterNewUser(newUser);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(expectedResponse, createdResult.Value);
        }

        [Fact]
        public async Task RegisterNewUser_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _controller.RegisterNewUser(new UserRegisterRequest());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task RegisterNewUser_ArgumentException_ReturnsConflict()
        {
            // Arrange
            var newUser = new UserRegisterRequest { /* inicializar propiedades válidas */ };
            _mockAuthService.Setup(service => service.RegisterNewUser(newUser))
                .ThrowsAsync(new ArgumentException("User already exists."));

            // Act
            var result = await _controller.RegisterNewUser(newUser);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("User already exists.", conflictResult.Value?.GetType().GetProperty("Message")?.GetValue(conflictResult.Value));
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResult()
        {
            // Arrange
            var userCredentials = new UserLoginRequest { /* inicializar propiedades válidas */ };
            var accessToken = "access_token";
            var refreshToken = "refresh_token";
            _mockAuthService.Setup(service => service.AutenticateAsync(userCredentials))
                .ReturnsAsync((accessToken, refreshToken));

            // Act
            var result = await _controller.Login(userCredentials);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(accessToken, okResult.Value.GetType().GetProperty("AccessToken")?.GetValue(okResult.Value));
            Assert.Equal(refreshToken, okResult.Value.GetType().GetProperty("RefreshToken")?.GetValue(okResult.Value));
        }

        [Fact]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Password", "Required");

            // Act
            var result = await _controller.Login(new UserLoginRequest());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_ValidToken_ReturnsOkResult()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";
            var accessToken = "new_access_token";
            _mockAuthService.Setup(service => service.RefreshTokenAsync(refreshToken))
                .ReturnsAsync(accessToken);

            // Act
            var result = await _controller.RefreshToken(refreshToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(accessToken, okResult.Value.GetType().GetProperty("AccessToken")?.GetValue(okResult.Value));
        }

        [Fact]
        public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var refreshToken = "invalid_refresh_token";
            _mockAuthService.Setup(service => service.RefreshTokenAsync(refreshToken))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid token."));

            // Act
            var result = await _controller.RefreshToken(refreshToken);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid token.", unauthorizedResult.Value?.GetType().GetProperty("Message")?.GetValue(unauthorizedResult.Value));
        }

        [Fact]
        public async Task Logout_ValidToken_ReturnsOkResult()
        {
            // Arrange
            var refreshToken = "valid_refresh_token";

            // Act
            var result = await _controller.Logout(refreshToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully logged out.", okResult.Value?.GetType().GetProperty("Message")?.GetValue(okResult.Value));
        }

        [Fact]
        public async Task Logout_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            string refreshToken = null;

            // Act
            var result = await _controller.Logout(refreshToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Refresh token is required.", badRequestResult.Value);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoListAPI.DTOs;
using ToDoListAPI.Services;
using BCrypt.Net;
using System.Net;

namespace ToDoListAPI.Controllers;

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

    [HttpPost] 
    public async Task<IActionResult> RegisterNewUser (UserRegisterRequest newUser)
    {

         try
        {
            var response = await _authService.RegisterNewUser(newUser);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return BadRequest(ex.Message);
        }
        
    }

    [HttpPost]
    public async Task<IActionResult> Login (UserLoginRequest userCredentials)
    {
        try
        {
            var response = await _authService.AutenticateAsync(userCredentials);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return Unauthorized(ex.Message);
        }

    }


}

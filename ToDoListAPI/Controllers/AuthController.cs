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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            var token = await _authService.RegisterNewUser(newUser);
            
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar nuevo usuario");
            return StatusCode((int)HttpStatusCode.InternalServerError, new { error = ex.Message});
        }

    }
}

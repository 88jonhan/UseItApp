using Microsoft.AspNetCore.Mvc;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;

namespace UseItApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest loginRequest)
    {
        var (success, errorMessage, response) = await authService.LoginAsync(loginRequest);

        if (!success)
        {
            return Unauthorized(new { message = errorMessage });
        }

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest registerRequest)
    {
        var (success, errorMessage, response) = await authService.RegisterAsync(registerRequest);

        if (!success)
        {
            return BadRequest(new { message = errorMessage });
        }

        return Ok(response);
    }
}
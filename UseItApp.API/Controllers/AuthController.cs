using Microsoft.AspNetCore.Mvc;
using UseItApp.API.Models;
using UseItApp.API.Services;
using UseItApp.Domain.Models;

namespace UseItApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest loginRequest)
    {
        var response = await authService.Login(loginRequest.Username, loginRequest.Password);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest registerRequest)
    {
        var user = new User
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            FirstName = registerRequest.FirstName,
            LastName = registerRequest.LastName
        };

        var response = await authService.Register(user, registerRequest.Password);

        if (response == null)
        {
            return BadRequest(new { message = "Username or email already exists" });
        }

        return Ok(response);
    }
}
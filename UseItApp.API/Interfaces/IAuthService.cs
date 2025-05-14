using UseItApp.API.Models;

namespace UseItApp.API.Interfaces;

public interface IAuthService
{
    Task<(bool success, string? errorMessage, AuthResponse? response)> LoginAsync(LoginRequest request);
    Task<(bool success, string? errorMessage, AuthResponse? response)> RegisterAsync(RegisterRequest request);
}
using UseItApp.Domain.Models;

namespace UseItApp.API.Models;

public class AuthResponse
{
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}
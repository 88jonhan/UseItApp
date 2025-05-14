using Microsoft.EntityFrameworkCore;
using UseItApp.API.Interfaces;
using UseItApp.API.Models;
using UseItApp.Data;
using UseItApp.Domain.Models;

namespace UseItApp.API.Services;

public class AuthService(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtGenerator jwtGenerator)
    : IAuthService
{
    public async Task<(bool success, string? errorMessage, AuthResponse? response)> LoginAsync(LoginRequest request)
    {
        // Validera input
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Username and password are required", null);
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
        {
            return (false, "Invalid username or password", null);
        }

        if (!passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            return (false, "Invalid username or password", null);
        }

        // Kontrollera om användaren är blockerad
        if (user.IsBlocked && user.BlockedUntil.HasValue && user.BlockedUntil.Value > DateTime.UtcNow)
        {
            return (false, $"User is blocked until {user.BlockedUntil.Value:yyyy-MM-dd HH:mm}. Reason: {user.BlockReason}", null);
        }

        var token = jwtGenerator.GenerateToken(user);
        var response = new AuthResponse
        {
            User = user,
            Token = token
        };

        return (true, null, response);
    }

    public async Task<(bool success, string? errorMessage, AuthResponse? response)> RegisterAsync(RegisterRequest request)
    {
        // Validera input
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Email))
        {
            return (false, "Username, email, and password are required", null);
        }

        // Validera e-postformat
        if (!IsValidEmail(request.Email))
        {
            return (false, "Invalid email format", null);
        }

        // Kontrollera om användarnamn redan finns
        if (await context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return (false, "Username already exists", null);
        }

        // Kontrollera om e-post redan finns
        if (await context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return (false, "Email already exists", null);
        }

        // Validera lösenordsstyrka (om du vill)
        if (request.Password.Length < 6)
        {
            return (false, "Password must be at least 6 characters long", null);
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsBlocked = false
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = jwtGenerator.GenerateToken(user);
        var response = new AuthResponse
        {
            User = user,
            Token = token
        };

        return (true, null, response);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
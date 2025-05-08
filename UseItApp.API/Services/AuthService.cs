using Microsoft.EntityFrameworkCore;
using UseItApp.API.Models;
using UseItApp.Data;
using UseItApp.Domain.Models;

namespace UseItApp.API.Services;

public interface IAuthService
{
    Task<AuthResponse?> Login(string username, string password);
    Task<AuthResponse?> Register(User user, string password);
}

public class AuthService(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtGenerator jwtGenerator)
    : IAuthService
{
    public async Task<AuthResponse?> Login(string username, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !passwordHasher.VerifyPassword(user.PasswordHash, password))
        {
            return null;
        }

        var token = jwtGenerator.GenerateToken(user);

        return new AuthResponse
        {
            User = user,
            Token = token
        };
    }

    public async Task<AuthResponse?> Register(User user, string password)
    {
        if (await context.Users.AnyAsync(u => u.Username == user.Username))
        {
            return null;
        }

        if (await context.Users.AnyAsync(u => u.Email == user.Email))
        {
            return null;
        }

        user.PasswordHash = passwordHasher.HashPassword(password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = jwtGenerator.GenerateToken(user);

        return new AuthResponse
        {
            User = user,
            Token = token
        };
    }
}
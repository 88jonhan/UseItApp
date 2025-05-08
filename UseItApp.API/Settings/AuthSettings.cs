namespace UseItApp.API.Settings;

public class AuthSettings
{
    public string Secret { get; set; } = string.Empty;
    public int TokenExpirationInHours { get; set; } = 24;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
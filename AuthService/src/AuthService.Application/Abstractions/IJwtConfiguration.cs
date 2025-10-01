namespace AuthService.Application.Abstractions;

public class IJwtConfiguration
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;  // symmetric secret
    public int ExpiryMinutes { get; init; } = 60;    // default 1 hour
}
namespace AuthService.Application.Abstractions;

public interface IJwtTokenProvider
{
    string GenerateToken(string userId, string username, string role);
}
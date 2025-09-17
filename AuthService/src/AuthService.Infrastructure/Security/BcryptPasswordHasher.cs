using AuthService.Application.Abstractions;

namespace AuthService.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string raw) => BCrypt.Net.BCrypt.HashPassword(raw);
    public bool Verify(string raw, string hash) => BCrypt.Net.BCrypt.Verify(raw, hash);
}
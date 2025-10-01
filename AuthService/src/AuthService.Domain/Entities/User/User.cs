
namespace AuthService.Domain.Entities.User;

public sealed class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public User(string username, string email, string passwordHash, UserRole role)
    {
        
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be null or whitespace.", nameof(username));
        if (username.Length < 3) throw new ArgumentException("Username must have at least 3 characters.", nameof(username));
        if (!username.All(ch => char.IsLetterOrDigit(ch) || ch is '_' or '.'))
            throw new ArgumentException("Username must contain only alphanumeric characters.", nameof(username));
        
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required");
        if (!email.Contains('@')) throw new ArgumentException("Invalid email");
        
        Id = Guid.NewGuid().ToString();
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }
}
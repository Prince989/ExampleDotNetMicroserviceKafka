using AuthService.Application.Abstractions;
using AuthService.Application.DTOs;
using AuthService.Domain.Entities.User;
using AuthService.Infrastructure.Interfaces;

namespace AuthService.Application.Services;

public class AuthHandler
{
    private readonly IUserRepository _dbUserRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    
    public AuthHandler(IPasswordHasher passwordHasher, IUserRepository dbUserRepository, IJwtTokenProvider jwtTokenProvider)
    {
        _passwordHasher = passwordHasher;
        _dbUserRepository = dbUserRepository;
        _jwtTokenProvider = jwtTokenProvider;
    }
    
    public async Task<User> AddAsync(SignUpDto data, CancellationToken ct)
    {
        var doesUsernameExist = await _dbUserRepository.CheckUniqueness(data.Username);

        if (doesUsernameExist)
        {
            throw new Exception("Username already exist");
        }
        
        var hashedPassword = _passwordHasher.Hash(data.Password);
        var role = new UserRole(data.Role);
        
        var user = new User(data.Username, data.Email, hashedPassword, role);
        
        await _dbUserRepository.InsertAsync(user);

        var resultUser = await _dbUserRepository.GetAsync(user.Id);
        
        return resultUser!;
    }

    public async Task<Dictionary<string, string>> LoginUser(LoginDto data, CancellationToken ct)
    {

        var user = await _dbUserRepository.FindByUsername(data.Username);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var checkPassword = _passwordHasher.Verify(data.Password, user.PasswordHash);
        
        if (!checkPassword)
        {
            throw new Exception("User not found");
        }

        var token = _jwtTokenProvider.GenerateToken(user.Id, user.Username, user.Role.Value);
        
        var result = new Dictionary<string, string>();
        
        result.Add("token", token);
        
        return result;
    }
}
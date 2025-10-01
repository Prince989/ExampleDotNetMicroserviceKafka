using AuthService.Domain.Entities;
using AuthService.Domain.Entities.User;

namespace AuthService.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<User?> GetAsync(string id);
    Task<IEnumerable<User>> GetAllAsync();
    Task InsertAsync(User entity);

    Task<bool> CheckUniqueness(string username);
    Task<User?> FindByUsername(string username);
    Task UpdateAsync(User entity, string id);
    Task DeleteAsync(User entity, string id);
}
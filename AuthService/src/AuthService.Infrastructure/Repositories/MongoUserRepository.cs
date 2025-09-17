using AuthService.Domain.Entities;
using AuthService.Domain.Entities.User;
using AuthService.Infrastructure.Interfaces;
using MongoDB.Driver;

namespace AuthService.Infrastructure.Repositories;

public class MongoUserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;
    
    public MongoUserRepository(IMongoDatabase db)
    {
        var collectionName = typeof(User).Name; // e.g., "User"
        _collection = db.GetCollection<User>(collectionName);
    }
    
    public async Task<bool> CheckUniqueness(string username)
    {
        var user = await FindByUsername(username);
        if (user == null)
            return false;
        return true;
    }
    
    public async Task<User?> FindByUsername(string username) => await _collection.Find(u => u.Username.Equals(username)).FirstOrDefaultAsync();
     
    public async Task<User?> GetAsync(string id) => await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<User>> GetAllAsync() => await _collection.Find(_ => true).ToListAsync();

    public Task InsertAsync(User entity) => _collection.InsertOneAsync(entity);

    public Task UpdateAsync(User entity, string id) => _collection.ReplaceOneAsync(t => t.Id == id, entity);

    public Task DeleteAsync(User entity, string id) =>  _collection.DeleteOneAsync(t => t.Id == id); 
}
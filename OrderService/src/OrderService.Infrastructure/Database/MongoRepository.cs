using MongoDB.Driver;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Domain.Order;

namespace OrderService.Infrastructure.Database;

public class MongoRepository : IRepository
{
    private readonly IMongoCollection<Order> _collection;

    public MongoRepository(IMongoDatabase db)
    {
        var collectionName = typeof(Order).Name;
        _collection = db.GetCollection<Order>(collectionName);
    }

    public async Task<Order?> GetByIdAsync(string id) => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    
    public Task<Order> InsertAsync(Order order) => _collection.InsertOneAsync(order).ContinueWith(o => order);
    
    public async Task<IEnumerable<Order>> GetMyOrdersAsync(string userId) => await _collection.Find(x => x.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Order>> GetSellerOrdersAsync(string sellerId) => await _collection.Find(x => x.SellerId == sellerId).ToListAsync();
}
using MongoDB.Driver;
using ProductService.Application.Abstractions;
using ProductService.Domain.Product;

namespace ProductService.Infrastructure.Database;

public class MongoProductRepository : IRepository
{
    private readonly IMongoCollection<Product> _collection;
    
    public MongoProductRepository(IMongoDatabase db)
    {
        var collectionName = typeof(Product).Name; // e.g., "Product"
        _collection = db.GetCollection<Product>(collectionName);
    }
    
    public Task<Product> InsertAsync(Product entity) => _collection.InsertOneAsync(entity).ContinueWith(t => entity);

    public Task<Product> UpdateAsync(Product entity) => _collection.ReplaceOneAsync(t => t.Id == entity.Id, entity).ContinueWith(t => entity);

    public Task DeleteAsync(string id) => _collection.DeleteOneAsync(t => t.Id == id);
    
    
    public async Task<Product?> GetByIdAsync(string id) => await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();

    public Task<IEnumerable<Product>> GetAllAsync() => _collection.Find(_ => true).ToListAsync().ContinueWith(t => (IEnumerable<Product>)t.Result);
    
    public Task<IEnumerable<Product>> GetSellerProducts(string sellerId) => _collection.Find(t => t.SellerId == sellerId).ToListAsync().ContinueWith(t => (IEnumerable<Product>)t.Result);
    
}
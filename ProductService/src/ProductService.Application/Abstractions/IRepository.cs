using ProductService.Domain.Product;

namespace ProductService.Application.Abstractions;

public interface IRepository
{
    Task<Product> InsertAsync (Product entity);
    Task<Product> UpdateAsync (Product entity);
    Task DeleteAsync (string id);
    Task<Product?> GetByIdAsync (string id);
    Task<IEnumerable<Product>> GetAllAsync ();
    
    Task<IEnumerable<Product>> GetSellerProducts (string sellerId);
}
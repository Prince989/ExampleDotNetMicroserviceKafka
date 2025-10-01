using ProductService.Application.Abstractions;
using ProductService.Domain.Product;

namespace ProductService.Application.Services;

public class FetchProductHandler
{
    private readonly IRepository _repository;

    public FetchProductHandler(IRepository repository) => _repository = repository;
    
    public async Task<Product?> FetchOne(string id) => await _repository.GetByIdAsync(id);
    
    public async Task<IEnumerable<Product>> FetchAll() => await _repository.GetAllAsync();

    public async Task<IEnumerable<Product>> FetchSellerProducts(string sellerId) => await _repository.GetSellerProducts(sellerId);
}
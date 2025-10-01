using ProductService.Application.Abstractions;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Domain.Product;

namespace ProductService.Application.Services;


public class CreateProductHandler
{
    private readonly IRepository _repository;
    private readonly IMessageProvider _messageProvider;
    
    public CreateProductHandler(IRepository repository, IMessageProvider messageProvider)
    {
        _repository = repository;
        _messageProvider = messageProvider;
    }

    public async Task<Product> Handle(ProductDto data, string sellerId)
    {
        if (String.IsNullOrWhiteSpace(data.Name))
            throw new ValidationException("Product name cannot be empty");

        var product = new Product(data.Name, data.Description, data.Price, data.Stock, sellerId);

        var result = await _repository.InsertAsync(product);

        await _messageProvider.PublishAsync("product.created", result);
        
        return result;
    }

}
using ProductService.Application.Abstractions;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Domain.Product;

namespace ProductService.Application.Services;

public class UpdateProductHandler
{
    private readonly IRepository _repository;
    private readonly IMessageProvider _messageProvider;

    
    public UpdateProductHandler(IRepository repository, IMessageProvider messageProvider)
    {
        _repository = repository;
        _messageProvider = messageProvider;
    }


    public async Task<Product> Handle(ProductDto data, string id, string sellerId)
    {
        if (String.IsNullOrWhiteSpace(data.Name))
            throw new ValidationException("Product name cannot be empty");

        var oldProduct = await _repository.GetByIdAsync(id);

        if (oldProduct.SellerId != sellerId)
            throw new ForbiddenException("This product doesn't belong to you");
        
        var product = new Product(data.Name, data.Description, data.Price, data.Stock, sellerId);

        
        var result = await _repository.UpdateAsync(product);

        await _messageProvider.PublishAsync("product.updated", result);
        
        return result;
    }
}
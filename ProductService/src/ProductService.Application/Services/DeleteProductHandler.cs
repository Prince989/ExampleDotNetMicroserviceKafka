using ProductService.Application.Abstractions;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Domain.Product;

namespace ProductService.Application.Services;

public class DeleteProductHandler
{
    private readonly IRepository _repository;
    private readonly IMessageProvider _messageProvider;

    
    public DeleteProductHandler(IRepository repository, IMessageProvider messageProvider)
    {
        _repository = repository;
        _messageProvider = messageProvider;
    }

    public async Task Handle(string id, string sellerId)
    {
        var oldProduct = await _repository.GetByIdAsync(id);

        if (oldProduct.SellerId != sellerId)
            throw new ForbiddenException("This product doesn't belong to you");
        
        
        await _repository.DeleteAsync(oldProduct.Id);

        await _messageProvider.PublishAsync("product.deleted", oldProduct);
        
        return;
    }
}
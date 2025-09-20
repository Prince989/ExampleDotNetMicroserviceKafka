using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using OrderService.Domain.Order;
using ProductService.Application.Common;

namespace OrderService.Application.Services;

public interface ICreateOrderHandler
{
    Task<Order> Handle(OrderDto data, string userId, CancellationToken ct);
}

public class CreateOrderHandler : ICreateOrderHandler
{
    private readonly IRepository _repository;
    private readonly IMessageProvider _messageProvider;
    private readonly IProductHttpClient _productHttpClient;

    public CreateOrderHandler(
        IRepository repository,
        IMessageProvider messageProvider,
        IProductHttpClient productHttpClient
    )
    {
        _repository = repository;
        _messageProvider = messageProvider;
        _productHttpClient = productHttpClient;
    }

    public async Task<Order> Handle(OrderDto data, string userId, CancellationToken ct)
    {
        if (data.postalCode == null || data.postalCode.Length < 10)
            throw new ValidationException("Postal code is missing or is not valid");
        if (data.address == null)
            throw new ValidationException("Address is missing");

        var product = await _productHttpClient.GetProductByIdAsync(data.productId, ct);

        if (product == null || product.Id == null)
            throw new NotFoundException("Product not found");

        var order = new Order(product.Name, product.Id, userId, product.SellerId, data.address, product.Price, data.postalCode);

        var createdOrder = await _repository.InsertAsync(order);

        await _messageProvider.ProduceAsync("order.created", createdOrder);

        return createdOrder;
    }
}
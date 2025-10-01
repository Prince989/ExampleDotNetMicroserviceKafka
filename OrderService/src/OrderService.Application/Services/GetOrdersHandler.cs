using OrderService.Application.Abstractions;
using OrderService.Domain.Order;

namespace OrderService.Application.Services;

public class GetOrdersHandler
{
    private readonly IRepository _repository;

    public GetOrdersHandler(IRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<IEnumerable<Order>> Handle(string userId, string role, CancellationToken ct)
    {
        IEnumerable<Order> orders;

        if (role == "Buyer")
            orders = await _repository.GetMyOrdersAsync(userId);
        else
            orders = await _repository.GetSellerOrdersAsync(userId);

        Console.WriteLine(role);
        
        return orders;
    }
}
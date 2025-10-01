using OrderService.Domain.Order;

namespace OrderService.Application.Abstractions;

public interface IRepository
{
    Task<Order> InsertAsync(Order order);
    Task<IEnumerable<Order>> GetMyOrdersAsync(string userId);
    Task<IEnumerable<Order>> GetSellerOrdersAsync(string sellerId);
    Task<Order?> GetByIdAsync(string id);
}
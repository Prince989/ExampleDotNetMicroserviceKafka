using OrderService.Application.DTOs;

namespace OrderService.Application.Abstractions;

public interface IProductHttpClient
{
    Task<ProductDto?> GetProductByIdAsync(string id, CancellationToken ct);
}
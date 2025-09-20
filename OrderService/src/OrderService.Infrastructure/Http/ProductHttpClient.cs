using System.Net.Http.Json;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;

namespace OrderService.Infrastructure.Http;

public class ProductHttpClient : IProductHttpClient
{
    private readonly HttpClient _httpClient;
    
    public ProductHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDto?> GetProductByIdAsync(string id, CancellationToken ct) => await _httpClient.GetFromJsonAsync<ProductDto>($"/api/Product/{id}", ct);
}
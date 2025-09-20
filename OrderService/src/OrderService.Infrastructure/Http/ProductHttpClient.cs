using System.Net;
using System.Net.Http.Json;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs;
using ProductService.Application.Common;

namespace OrderService.Infrastructure.Http;

public class ProductHttpClient : IProductHttpClient
{
    private readonly HttpClient _httpClient;
    
    public ProductHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<ProductDto?> GetProductByIdAsync(string id, CancellationToken ct)
    {
        var resp = await _httpClient.GetAsync($"/api/Product/{id}", ct);

        if (resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.NoContent)
            throw new NotFoundException("Product not found");

        resp.EnsureSuccessStatusCode();

        var dto = await resp.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
        if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
            throw new NotFoundException("Product not found");

        return dto;
    }
}
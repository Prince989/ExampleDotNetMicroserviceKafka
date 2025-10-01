namespace OrderService.Application.DTOs;

public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string SellerId { get; set; }
    public decimal Price { get; set; }
}
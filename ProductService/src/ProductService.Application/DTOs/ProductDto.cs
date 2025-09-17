namespace ProductService.Application.DTOs;

public class ProductDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public int Stock { get; set; } = 0;
}
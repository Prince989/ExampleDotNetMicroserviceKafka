namespace OrderService.Application.DTOs;

public class OrderDto
{
    public string productId { get; set; } = string.Empty;
    public int quantity { get; set; } = 0;
    public string address { get; set; } = string.Empty;
    public string postalCode { get; set; } = string.Empty;
}
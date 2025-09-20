namespace OrderService.Application.DTOs;

public class OrderDto
{
    public string productId { get; set; } = string.Empty;
    public string address { get; set; } = string.Empty;
    public string postalCode { get; set; } = string.Empty;
    public string userId { get; set; } = string.Empty;
}
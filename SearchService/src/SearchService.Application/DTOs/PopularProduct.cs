namespace SearchService.Application.DTOs;

public class PopularProduct
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public double OrdersCount { get; set; } = default;

    public PopularProduct(
        string productId,
        string productName,
        double ordersCount 
    )
    {
        ProductId = productId;
        ProductName = productName;
        OrdersCount = ordersCount;
    }
}
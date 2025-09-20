namespace OrderService.Domain.Order;

public class Order
{
    public string Id { get; set; }
    public string ProductName { get; set; }
    public string ProductId { get; set; }
    public string UserId { get; set; }
    public decimal Price { get; set; }
    public string SellerId { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Order(
        string productName,
        string productId,
        string userId,
        string sellerId,
        string address,
        int quantity,
        decimal price,
        string postalCode
    )
    {
        Id = Guid.NewGuid().ToString();
        ProductName = productName;
        ProductId = productId;
        UserId = userId;
        SellerId = sellerId;
        Address = address;
        Price = price;
        Quantity = quantity;
        PostalCode = postalCode;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }
}
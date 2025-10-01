namespace ProductService.Domain.Product;

public sealed class Product
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; } 
    public int Stock { get; set; } 
    
    public string SellerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public Product(string name, string description, decimal price, int stock, string sellerId)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        SellerId = sellerId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
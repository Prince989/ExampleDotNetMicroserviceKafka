namespace SearchService.Domain.Entities;

public class ProductDocument
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; } 
    public int Stock { get; set; }
    public string SellerId { get; set; }    
}
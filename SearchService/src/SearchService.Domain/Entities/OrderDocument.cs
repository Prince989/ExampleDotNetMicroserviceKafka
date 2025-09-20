namespace SearchService.Domain.Entities;

public class OrderDocument
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
}
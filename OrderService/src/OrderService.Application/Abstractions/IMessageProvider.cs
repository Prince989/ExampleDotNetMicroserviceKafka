namespace OrderService.Application.Abstractions;

public interface IMessageProvider
{ 
    Task ProduceAsync(string title, object payload);
}
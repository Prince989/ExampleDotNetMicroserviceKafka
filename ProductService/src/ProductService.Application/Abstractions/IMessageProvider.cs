namespace ProductService.Application.Abstractions;

public interface IMessageProvider
{
    Task PublishAsync(string title, object payload);
}
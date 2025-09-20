namespace SearchService.Application.Abstractions;

public interface IIndexRepository
{
    Task IndexAsync<T>(T doc, CancellationToken ct = default) where T : class;
    Task DeleteAsync<T>(string id, CancellationToken ct = default) where T : class;
}
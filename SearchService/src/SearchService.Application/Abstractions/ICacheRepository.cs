namespace SearchService.Application.Abstractions;

public interface ICacheRepository
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);

    Task<string> GetVersionAsync(string scope, CancellationToken ct = default);
    Task BumpVersionAsync(string scope, CancellationToken ct = default);
}
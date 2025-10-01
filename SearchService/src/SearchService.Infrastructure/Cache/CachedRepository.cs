using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SearchService.Application.Abstractions;

namespace SearchService.Infrastructure.Cache;

public sealed class CachedRepository : ICacheRepository
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public CachedRepository(IDistributedCache cache) => _cache = cache;

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is null || bytes.Length == 0) return default;
        return JsonSerializer.Deserialize<T>(bytes, _json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _json);
        var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
        await _cache.SetAsync(key, bytes, opts, ct);
    }
    
    public Task RemoveAsync(string key, CancellationToken ct = default) => _cache.RemoveAsync(key, ct);

    public async Task<string> GetVersionAsync(string scope, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(scope, ct);
        if (bytes is { Length: > 0 }) return System.Text.Encoding.UTF8.GetString(bytes);
        var ver = Guid.NewGuid().ToString("N");
        await _cache.SetStringAsync(scope, ver, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        }, ct);
        return ver;
    }

    public async Task BumpVersionAsync(string scope, CancellationToken ct = default)
    {
        var ver = Guid.NewGuid().ToString("N");
        await _cache.SetStringAsync(scope, ver, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        }, ct);
    }
}

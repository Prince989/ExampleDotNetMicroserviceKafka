using System.Security.Cryptography;
using System.Text;

namespace SearchService.Application.Common;

public static class CacheScopes
{
    public const string Products = "cache:ver:products";
    public const string Orders   = "cache:ver:orders";
}

public static class CacheKeys
{
    public static string SearchProductsKey(string version, string q, decimal min, decimal max, object sorting)
    {
        var raw = $"v={version}|q={q}|min={min}|max={max}|sort={sorting}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();
        return $"cache:search:products:{hash}";
    }

    public static string PopularProductsKey(string version, int size)
    {
        return $"cache:agg:popular-products:v={version}:size={size}";
    }
}
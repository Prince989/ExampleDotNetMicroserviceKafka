using Nest;
using SearchService.Application.Abstractions;
using SearchService.Application.Common;
using SearchService.Application.DTOs;
using SearchService.Domain.Entities;
using SearchService.Domain.SortingType;

namespace SearchService.Infrastructure.Elastic;

public class ElasticSearchRepository : ISearchRepository
{
    private IElasticClient _elasticClient;
    private readonly ICacheRepository _cache;
    private static readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(1);

    public ElasticSearchRepository(IElasticClient elasticClient, ICacheRepository cache)
    {
        _elasticClient = elasticClient;
        _cache = cache;
    }
    
    private Task BumpScopeFor<T>(CancellationToken ct)
    {
        var t = typeof(T);
        if (t == typeof(ProductDocument)) return _cache.BumpVersionAsync(CacheScopes.Products, ct);
        if (t == typeof(OrderDocument))   return _cache.BumpVersionAsync(CacheScopes.Orders, ct);
        
        return Task.CompletedTask;
    }

    public async Task IndexAsync<T>(T doc, CancellationToken ct = default) where T : class
    {
        var resp = await _elasticClient.IndexDocumentAsync(doc, ct);
        if (!resp.IsValid) throw new Exception($"Elasticsearch index failed: {resp.ServerError}");

        // invalidation وابسته به نوع
        await BumpScopeFor<T>(ct);
    }

    public async Task DeleteAsync<T>(string id, CancellationToken ct = default) where T : class
    {
        var resp = await _elasticClient.DeleteAsync<T>(id);
        if (!resp.IsValid) throw new Exception($"Elasticsearch delete failed: {resp.ServerError}");

        await BumpScopeFor<T>(ct);
    }

    public async Task<IEnumerable<ProductDocument>> SearchAsync(string q, decimal minPrice, decimal maxPrice,
        SortingType sorting, CancellationToken ct = default)
    {
        var version = await _cache.GetVersionAsync(CacheScopes.Products, ct);
        var key = CacheKeys.SearchProductsKey(version, q, minPrice, maxPrice, sorting);

        var cached = await _cache.GetAsync<IEnumerable<ProductDocument>>(key, ct);
        if (cached is not null)
        {
            Console.WriteLine("Cache hit");
            return cached;
        }
        
        var resp = await _elasticClient.SearchAsync<ProductDocument>(s => s.Index("products")
                .Query(qr => qr.Bool(b => b.Must(m =>
                            m.MultiMatch(mm => mm.Query(q).Fields(f => f.Field(p => p.Name
                                    ).Field(p => p.Description)
                                )
                            )
                        )
                        .Filter(f => f.Range(r =>
                            r.Field(p => p.Price).GreaterThanOrEquals((double?)minPrice)
                                .LessThanOrEquals((double?)maxPrice)))
                    )
                ).Sort(ss => sorting switch
                {
                    SortingType.NameAsc => ss.Field(f => f.Field("name.kw").Order(SortOrder.Ascending)),
                    SortingType.NameDsc => ss.Field(f => f.Field("name.kw").Order(SortOrder.Descending)),
                    SortingType.PriceAsc => ss.Ascending(p => p.Price),
                    SortingType.PriceDsc => ss.Descending(p => p.Price)
                }).Size(20),
            ct
        );

        if (!resp.IsValid)
        {
            Console.WriteLine(resp.DebugInformation);
            Console.WriteLine(resp.ServerError?.ToString());
            throw new Exception("Elasticsearch query failed");
        }

        var result = resp.Documents.ToArray();
        await _cache.SetAsync(key, result, _defaultTtl, ct);
        
        return resp.Documents;
    }

    public async Task<IEnumerable<PopularProduct>> PopularProducts(int size = 10)
    {
        var version = await _cache.GetVersionAsync(CacheScopes.Orders);
        var key = CacheKeys.PopularProductsKey(version, size);
        
        var cached = await _cache.GetAsync<IEnumerable<PopularProduct>>(key);
        if (cached is not null){
            Console.WriteLine("Cache hit");
            return cached;
        }

        var resp = await _elasticClient.SearchAsync<OrderDocument>(s => s.Index("orders")
            .Size(0)
            .Aggregations(a => a.Terms("by_product",
                    t => t.Field("productId.keyword")
                        .Size(size)
                        .Order(o => o.Descending("total_quantity"))
                        .Aggregations(aa =>
                            aa.TopHits("sample", tt => tt.Size(1).Source(sf => sf.Includes(i => i.Fields("productName"))))
                                .Sum("total_quantity", sm => sm.Field("quantity"))
                        )
                )
            )
        );
        if (!resp.IsValid)
            throw new NotFoundException($"Elasticsearch agg failed: {resp.ServerError}");

        var buckets = resp.Aggregations.Terms("by_product")?.Buckets;
        if (buckets == null || buckets.Count == 0)
        {
            Console.WriteLine("Empty Results");            
            return Enumerable.Empty<PopularProduct>();
        }

        var results = buckets.Select(b =>
        {
            var topHitsAgg = b.TopHits("sample");
            var firstHit   = topHitsAgg?.Hits<OrderDocument>().FirstOrDefault();
            var productName = firstHit?.Source?.ProductName ?? string.Empty;

            var totalQty = b.Sum("total_quantity")?.Value ?? 0;
            
            return new PopularProduct(
                b.Key,
                productName,
                totalQty
            );
        }).ToArray();
        
        
        await _cache.SetAsync(key, results, _defaultTtl);
        return results;
    }
}
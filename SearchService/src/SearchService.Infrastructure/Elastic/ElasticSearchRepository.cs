using Nest;
using SearchService.Application.Abstractions;
using SearchService.Domain.Entities;
using SearchService.Domain.SortingType;

namespace SearchService.Infrastructure.Elastic;

public class ElasticSearchRepository : ISearchRepository
{
    private IElasticClient _elasticClient;

    public ElasticSearchRepository(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task IndexAsync<T>(T doc, CancellationToken ct = default) where T : class
    {
        await _elasticClient.IndexDocumentAsync(doc, ct);
    }

    public async Task DeleteAsync<T>(string id, CancellationToken ct = default) where T : class
    {
        await _elasticClient.DeleteAsync<T>(id);
    }

    public async Task<IEnumerable<ProductDocument>> SearchAsync(string q, decimal minPrice, decimal maxPrice,
        SortingType sorting, CancellationToken ct = default)
    {
        var resp = await _elasticClient.SearchAsync<ProductDocument>(s => s.Index("products")
            .Query(qr => qr.Bool(b => b.Must(m =>
                        m.MultiMatch(mm => mm.Query(q).Fields(f => f.Field(p => p.Name
                                ).Field(p => p.Description)
                            )
                        )
                    )
                    .Filter(f => f.Range(r => r.Field(p => p.Price).GreaterThanOrEquals((double?)minPrice).LessThanOrEquals((double?)maxPrice)))
                )
            ).Sort(ss => sorting switch
            {
                SortingType.NameAsc  => ss.Field(f => f.Field("name.kw").Order(SortOrder.Ascending)),
                SortingType.NameDsc  => ss.Field(f => f.Field("name.kw").Order(SortOrder.Descending)),
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
        
        return resp.Documents;
    }
}
using Nest;
using Microsoft.Extensions.Logging;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Elastic;

public static class ElasticBootstrapper
{
    private const string ProductsAlias = "products";
    private const string OrdersAlias = "orders";

    public static async Task BootstrapAsync(IElasticClient es, ILogger logger, CancellationToken ct = default)
    {
        await EnsureProductsAsync(es, logger, ct);
        await EnsureOrdersAsync(es, logger, ct);
    }

    private static async Task EnsureProductsAsync(IElasticClient es, ILogger logger, CancellationToken ct)
    {
        const string aliasName = "products";

        var aliasResp = await es.Indices.GetAliasAsync(aliasName, d => d, ct);

        if (aliasResp.IsValid)
        {
            var indices = aliasResp.Indices.Keys.Select(k => k.Name).ToArray();
            var current = indices.First();

            var map = await es.Indices.GetMappingAsync<ProductDocument>(m => m.Index(current), ct);
            if (!map.IsValid) throw new Exception($"GetMapping failed: {map.ServerError}");

            var props = map.Indices[current].Mappings.Properties;
            if (props.TryGetValue("name", out var nameProp)
                && nameProp is ITextProperty tp && tp.Fields != null && tp.Fields.ContainsKey("kw"))
            {
                logger.LogInformation("Products mapping OK on {Index}", current);
                return;
            }

            await MigrateProductsIndexAsync(es, logger, sourceIndexOrAlias: aliasName, aliasName, ct);
            return;
        }

        if (aliasResp.ServerError?.Status == 404)
        {
            var exists = await es.Indices.ExistsAsync(aliasName);
            if (exists.Exists)
            {
                await MigrateProductsIndexAsync(es, logger, sourceIndexOrAlias: aliasName, aliasName, ct);
                return;
            }

            var newIndex = $"{aliasName}-v1";
            logger.LogInformation("Creating products index {Index} with alias {Alias}", newIndex, aliasName);

            var create = await es.Indices.CreateAsync(newIndex, c => c
                    .Map<ProductDocument>(m => m.Properties(p => p
                        .Keyword(k => k.Name(n => n.Id))
                        .Text(t => t.Name(n => n.Name)
                            .Fields(ff => ff.Keyword(kk => kk.Name("kw").IgnoreAbove(256))))
                        .Text(t => t.Name(n => n.Description))
                        .Number(nu => nu.Name(n => n.Price).Type(NumberType.Double))
                    ))
                    .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0))
                , ct);
            if (!create.IsValid) throw new Exception($"Failed to create products index: {create.ServerError}");

            var putAlias = await es.Indices.PutAliasAsync(newIndex, aliasName);
            if (!putAlias.IsValid) throw new Exception($"Failed to create alias {aliasName}: {putAlias.ServerError}");

            logger.LogInformation("Products alias now points to {Index}", newIndex);
            return;
        }

        throw new Exception($"GetAlias({aliasName}) failed: {aliasResp.ServerError}");
    }

    private static async Task MigrateProductsIndexAsync(IElasticClient es, ILogger logger, string sourceIndexOrAlias,
        string aliasName, CancellationToken ct)
    {
        var newIndex = $"{aliasName}-v{DateTime.UtcNow:yyyyMMddHHmmss}";
        logger.LogWarning("Migrating products → creating {New}", newIndex);

        var createNew = await es.Indices.CreateAsync(newIndex, c => c
                .Map<ProductDocument>(m => m.Properties(p => p
                    .Keyword(k => k.Name(n => n.Id))
                    .Text(t => t.Name(n => n.Name)
                        .Fields(ff => ff.Keyword(kk => kk.Name("kw").IgnoreAbove(256))))
                    .Text(t => t.Name(n => n.Description))
                    .Number(nu => nu.Name(n => n.Price).Type(NumberType.Double))
                ))
                .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0))
            , ct);
        if (!createNew.IsValid) throw new Exception($"Create new products index failed: {createNew.ServerError}");

        var reindex = await es.ReindexOnServerAsync(r => r
                .Source(s => s.Index(sourceIndexOrAlias))
                .Destination(d => d.Index(newIndex))
                .WaitForCompletion(true)
            , ct);
        if (!reindex.IsValid) throw new Exception($"Reindex failed: {reindex.ServerError}");

        var exists = await es.Indices.ExistsAsync(aliasName);
        if (exists.Exists)
        {
            var del = await es.Indices.DeleteAsync(aliasName);
            if (!del.IsValid) throw new Exception($"Delete old index '{aliasName}' failed: {del.ServerError}");
        }
        else
        {
            var currentAliases = await es.Indices.GetAliasAsync(aliasName, d => d, ct);
            if (currentAliases.IsValid)
            {
                foreach (var oldIndex in currentAliases.Indices.Keys.Select(k => k.Name))
                {
                    var delAlias = await es.Indices.DeleteAliasAsync(oldIndex, aliasName);
                    if (!delAlias.IsValid && delAlias.ServerError?.Status != 404)
                        throw new Exception(
                            $"DeleteAlias '{aliasName}' from '{oldIndex}' failed: {delAlias.ServerError}");
                }
            }
        }

        var put = await es.Indices.PutAliasAsync(newIndex, aliasName);
        if (!put.IsValid) throw new Exception($"Alias swap failed: {put.ServerError}");

        logger.LogInformation("Alias {Alias} now points to {Index}", aliasName, newIndex);
    }

    private static async Task EnsureOrdersAsync(IElasticClient es, ILogger logger, CancellationToken ct)
    {
        var aliasResp = await es.Indices.GetAliasAsync(OrdersAlias, d => d, ct);
        if (aliasResp.IsValid) return;

        if (aliasResp.ServerError?.Status == 404)
        {
            var indexName = $"{OrdersAlias}-v1";
            logger.LogInformation("Creating orders index {Index} with alias {Alias}", indexName, OrdersAlias);

            var create = await es.Indices.CreateAsync(indexName, c => c
                    .Map<OrderDocument>(m => m.Properties(p => p
                        .Keyword(k => k.Name(n => n.Id))
                        .Keyword(k => k.Name(n => n.ProductId))
                        .Keyword(k => k.Name(n => n.SellerId))
                        .Text(t => t.Name(n => n.ProductName)
                            .Fields(ff => ff.Keyword(kk => kk.Name("kw").IgnoreAbove(256))))
                        .Number(nu => nu.Name(n => n.Quantity).Type(NumberType.Integer))
                    ))
                    .Aliases(a => a.Alias(OrdersAlias))
                    .Settings(s => s.NumberOfShards(1).NumberOfReplicas(0))
                , ct);

            if (!create.IsValid) throw new Exception($"Failed to create orders index: {create.ServerError}");
        }
        else
        {
            throw new Exception($"GetAlias({OrdersAlias}) failed: {aliasResp.ServerError}");
        }
    }
}
using SearchService.Domain.Entities;
using SearchService.Domain.SortingType;

namespace SearchService.Application.Abstractions;

public interface IProductSearchRepository
{
    Task<IEnumerable<ProductDocument>> SearchAsync(
        string q, decimal minPrice, decimal maxPrice, SortingType sorting, CancellationToken ct = default);
}
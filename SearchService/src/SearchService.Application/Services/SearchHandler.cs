using SearchService.Application.Abstractions;
using SearchService.Application.Common;
using SearchService.Application.DTOs;
using SearchService.Domain.Entities;
using SearchService.Domain.SortingType;

namespace SearchService.Application.Services;

public class SearchHandler
{
    private readonly ISearchRepository _repository;

    public SearchHandler(ISearchRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductDocument>> SearchProducts(
        string q,
        decimal? minPrice,
        decimal? maxPrice,
        SortingType sorting = SortingType.NameAsc
    )
    {
        if (q == null)
            throw new ValidationException("q cannot be empty or null");
        
        var min = minPrice ?? 0m;
        var max = maxPrice ?? decimal.MaxValue;

        if (min < 0)
            throw new ValidationException("minPrice cannot be negative");
        if (max < min)
            throw new ValidationException("maxPrice cannot be less than minPrice");
        
        var results = await _repository.SearchAsync(q,min,max,sorting);
        
        return results;
    }

    public async Task<IEnumerable<PopularProduct>> GetPopularProducts(int size)
    {
        var results = await _repository.PopularProducts(size);
        
        return results;
    }
}
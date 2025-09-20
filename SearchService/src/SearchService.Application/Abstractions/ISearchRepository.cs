using SearchService.Domain.Entities;
using SearchService.Domain.SortingType;

namespace SearchService.Application.Abstractions;

public interface ISearchRepository : IIndexRepository , IProductSearchRepository
{ }
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Services;
using SearchService.Domain.SortingType;

namespace SearchService.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly SearchHandler _searchHandler;

    public SearchController(SearchHandler searchHandler)
    {
        _searchHandler = searchHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] SortingType sorting)
    {
        var results = await _searchHandler.SearchProducts(q, minPrice, maxPrice, sorting);
        return Ok(results);
    }

    [HttpGet]
    [Route("top")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPopularProducts(
        [FromQuery] int size = 10
    )
    {
        var results = await _searchHandler.GetPopularProducts(size);
        
        return Ok(results);
    }
    
}
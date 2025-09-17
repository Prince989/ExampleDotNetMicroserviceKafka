using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Repository;
using ProductService.Domain.Product;

namespace ProductService.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private CreateProductHandler _createProductHandler;
    private UpdateProductHandler _updateProductHandler;
    private FetchProductHandler _fetchProductHandler;
    private DeleteProductHandler _deleteProductHandler;
    
    public ProductController(
        CreateProductHandler createProductHandler,
        UpdateProductHandler updateProductHandler,
        FetchProductHandler fetchProductHandler,
        DeleteProductHandler deleteProductHandler    
    )
    {
        _createProductHandler = createProductHandler;
        _updateProductHandler = updateProductHandler;
        _fetchProductHandler = fetchProductHandler;
        _deleteProductHandler = deleteProductHandler;
    }

    [Authorize(Roles = "Seller")]
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProductDto product)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User id missing in token");

        var result = await _createProductHandler.Handle(product, userId);

        return Ok(result);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Put([FromBody] ProductDto product, [FromRoute] string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User id missing in token");

        var result = await _updateProductHandler.Handle(product, id, userId);

        return Ok(result);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> Get([FromRoute] string id)
    {
        var result = await _fetchProductHandler.FetchOne(id);

        return Ok(result);
    }
    
    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User id missing in token");

        await _deleteProductHandler.Handle(id, userId);
        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _fetchProductHandler.FetchAll();

        return Ok(result);
    }
    
    [Authorize(Roles = "Seller")]
    [HttpGet]
    [Route("my")]
    public async Task<IActionResult> GetMy()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException("User id missing in token");

        var result = await _fetchProductHandler.FetchSellerProducts(userId);

        return Ok(result);
    }
    
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Services;
using ProductService.Application.Common;

namespace OrderService.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly CreateOrderHandler _createOrderHandler;
    private readonly GetOrdersHandler _getOrderHandler;

    public OrderController(
        CreateOrderHandler createOrderHandler,
        GetOrdersHandler getOrderHandler
    )
    {
        _createOrderHandler = createOrderHandler;
        _getOrderHandler = getOrderHandler;
    }

    [Authorize(Roles = "Buyer")]
    [HttpPost]
    public async Task<IActionResult> PostOrder([FromBody] OrderDto order, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedException("User id missing in token");

        var result = await _createOrderHandler.Handle(order, userId, ct);

        return Ok(result);
    }

    [Authorize(Roles = "Buyer,Seller")]
    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedException("User id missing in token");
        var role = User.FindFirstValue(ClaimTypes.Role)
            ?? throw new ForbiddenException("Access Forbidden");
        
        var result = await _getOrderHandler.Handle( userId, role, ct);

        return Ok(result);
    }
}
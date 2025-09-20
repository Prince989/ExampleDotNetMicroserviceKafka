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
    private readonly ICreateOrderHandler _createOrderHandler;
    private readonly IGetOrdersHandler _getOrderHandler;

    public OrderController(
        ICreateOrderHandler createOrderHandler,
        IGetOrdersHandler getOrderHandler
    )
    {
        _createOrderHandler = createOrderHandler;
        _getOrderHandler = getOrderHandler;
    }

    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> PostOrder([FromBody] OrderDto order, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedException("User id missing in token");

        var result = await _createOrderHandler.Handle(order, userId, ct);

        return Ok(result);
    }

    [Authorize(Roles = "Buyer,Seller")]
    public async Task<IActionResult> GetOrders([FromBody] OrderDto order, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedException("User id missing in token");
        var role = User.FindFirstValue(ClaimTypes.Role)
            ?? throw new ForbiddenException("Access Forbidden");
        
        var result = await _getOrderHandler.Handle(role, userId, ct);

        return Ok(result);
    }
}
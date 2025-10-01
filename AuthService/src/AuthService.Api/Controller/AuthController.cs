using AuthService.Application.Abstractions;
using AuthService.Application.DTOs;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthHandler _service;

    public AuthController(AuthHandler service)
    {
        _service = service;
    }

    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok("Auth Service is running.");
    }

    [HttpGet("delay/{seconds}")]
    public async Task<IActionResult> Delay(int seconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        return Ok(new { message = $"Delayed for {seconds} seconds" });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUpAsync([FromBody] SignUpDto data, CancellationToken ct)
    {
        var user = await _service.AddAsync(data, ct);

        return Ok(user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDto data, CancellationToken ct)
    {
        var token = await _service.LoginUser(data, ct);

        return Ok(token);
    }
}
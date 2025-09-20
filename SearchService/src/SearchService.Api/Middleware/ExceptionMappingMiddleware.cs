using System.Net;
using System.Text.Json;
using SearchService.Application.Common;

namespace SearchService.Api.Middleware;

public sealed class ExceptionMappingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMappingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ExceptionMappingMiddleware(RequestDelegate next, ILogger<ExceptionMappingMiddleware> logger)
        => (_next, _logger) = (next, logger);

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (AppException ex) // your controlled errors
        {
            await WriteProblem(ctx, ex.StatusCode, ex.ErrorCode, ex.Message, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteProblem(ctx, StatusCodes.Status401Unauthorized, "unauthorized", ex.Message, ex);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteProblem(ctx, StatusCodes.Status404NotFound, "not_found", ex.Message, ex);
        }
        catch (Exception ex) // fallback 500
        {
            await WriteProblem(ctx, StatusCodes.Status500InternalServerError, "internal_error", "An unexpected error occurred.", ex);
        }
    }

    private async Task WriteProblem(HttpContext ctx, int status, string code, string message, Exception ex)
    {
        var traceId = ctx.TraceIdentifier;
        _logger.LogError(ex, "HTTP {Status} {Code} {TraceId} {Method} {Path}", status, code, traceId, ctx.Request.Method, ctx.Request.Path);

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        var payload = new
        {
            error = new {
                code,
                message,
                traceId
            }
        };

        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOpts));
    }
}

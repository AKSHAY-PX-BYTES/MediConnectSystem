using System.Net;
using System.Text.Json;
using MediConnect.Application.Common.Exceptions;

namespace MediConnect.WebApi.Middleware;

/// <summary>
/// Converts application exceptions into consistent RFC 7807-style JSON problem
/// responses and prevents leaking internal details to clients.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, title, errors) = ex switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, "Validation failed.", ve.Errors),
            NotFoundException => (HttpStatusCode.NotFound, ex.Message, null),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, ex.Message, null),
            ConflictException => (HttpStatusCode.Conflict, ex.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var payload = new
        {
            status = (int)status,
            title,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

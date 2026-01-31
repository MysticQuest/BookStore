using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent error responses.
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict,
                "The record was modified by another user. Please refresh and try again."),
            
            DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("UNIQUE constraint") == true => (
                HttpStatusCode.Conflict,
                "A record with this identifier already exists."),
            
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                argEx.Message),
            
            InvalidOperationException invEx => (
                HttpStatusCode.BadRequest,
                invEx.Message),
            
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "The requested resource was not found."),
            
            OperationCanceledException => (
                HttpStatusCode.BadRequest,
                "The operation was cancelled."),
            
            HttpRequestException httpEx => (
                HttpStatusCode.BadGateway,
                "An error occurred while communicating with an external service."),
            
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.")
        };

        _logger.LogError(exception, 
            "Unhandled exception occurred. Status: {StatusCode}, Message: {Message}", 
            (int)statusCode, message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Status = (int)statusCode,
            Message = message,
            TraceId = context.TraceIdentifier
        };

        if (_environment.IsDevelopment())
        {
            response.Detail = exception.ToString();
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Standard error response format.
/// </summary>
public class ErrorResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? Detail { get; set; }
}

/// <summary>
/// Extension methods for registering the global exception handler.
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}

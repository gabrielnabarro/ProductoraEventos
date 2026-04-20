using Application.DTOs;
using Domain.Exceptions;
using System.Net;

namespace EventsApi.Middlewares;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        catch (DomainException exception)
        {
            await WriteErrorAsync(context, (int)exception.StatusCode, exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
            await WriteErrorAsync(context, (int)HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(new ErrorResponseDto
        {
            Message = message,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        });
    }
}

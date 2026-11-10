using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TakweneTask.Api.Middleware;

/// <summary>
/// Centralized RFC 9457 exception handler strictly reserved for genuine, unhandled infrastructure
/// or system-level exceptions (e.g. database connection drops, concurrency collisions, or unexpected crashes).
/// Expected business and domain outcomes (NotFound, Conflict, Unauthorized) are modeled via Result&lt;T&gt;.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title, detail) = exception switch
        {
            DbUpdateException dbUpdateEx when IsUniqueConstraintViolation(dbUpdateEx) => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "A record with the specified unique identifier or key already exists."
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred processing your request."
            )
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled system exception occurred. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning("Database constraint violation occurred with status {StatusCode}: {Detail}. TraceId: {TraceId}",
                statusCode, detail, traceId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        if (exception.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return true;
        }

        return false;
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TakweneTask.Application.Common.Models;

namespace TakweneTask.Api.Extensions;

public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        int successStatusCode = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            if (successStatusCode == StatusCodes.Status200OK)
            {
                return controller.Ok(result.Value);
            }

            return controller.StatusCode(successStatusCode, result.Value);
        }

        var (statusCode, title) = result.ErrorType switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Domain Rule Violation"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var traceId = Activity.Current?.Id ?? controller.HttpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = result.ErrorMessage,
            Instance = controller.HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    public static IActionResult ToGenericActionResult<T>(
        this Result<T> result,
        ControllerBase controller,
        int successStatusCode = StatusCodes.Status200OK)
    {
        var actionResult = result.ToActionResult(controller, successStatusCode);
        return actionResult.Result ?? new OkObjectResult(actionResult.Value);
    }
}

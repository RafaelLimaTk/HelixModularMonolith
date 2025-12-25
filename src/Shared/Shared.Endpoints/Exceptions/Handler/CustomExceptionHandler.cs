using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Application.Exceptions;
using Shared.Domain.Exceptions;

namespace Shared.Endpoints.Exceptions.Handler;

public class CustomExceptionHandler
    (ILogger<CustomExceptionHandler> logger,
        IHostEnvironment hostEnvironment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(
            "Error Message: {exceptionMessage}, Time of occurrence {time}",
            exception.Message, DateTime.UtcNow);

        var details = new ProblemDetails();

        if (hostEnvironment.IsDevelopment())
            details.Extensions.Add("StackTrace", exception.StackTrace);

        switch (exception)
        {
            case ValidationException validationException:
                details.Title = "One or more validation errors occurred.";
                details.Status = StatusCodes.Status422UnprocessableEntity;
                details.Type = "UnprocessableEntity";
                details.Detail = exception.Message;
                details.Extensions.Add("ValidationErrors", validationException.Errors);
                break;

            case EntityValidationException:
                details.Title = "One or more validation errors occurred.";
                details.Status = StatusCodes.Status422UnprocessableEntity;
                details.Type = "UnprocessableEntity";
                details.Detail = exception.Message;
                break;

            case NotFoundException:
                details.Title = "Not Found";
                details.Status = StatusCodes.Status404NotFound;
                details.Type = "NotFound";
                details.Detail = exception.Message;
                break;

            default:
                details.Title = "An unexpected error occurred";
                details.Status = StatusCodes.Status500InternalServerError;
                details.Type = "UnexpectedError";
                details.Detail = exception.Message;
                break;
        }

        context.Response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(details, cancellationToken: cancellationToken);
        return true;
    }
}

using Microsoft.AspNetCore.Diagnostics;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.SharedKernel;

namespace RouteFlow.Api.ExceptionHandling;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            DeliveryNotFoundException => (StatusCodes.Status404NotFound, "Delivery not found"),
            DeliveryConcurrencyException => (StatusCodes.Status409Conflict, "Delivery update conflict"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Invalid delivery operation"),
            ArgumentException => (StatusCodes.Status422UnprocessableEntity, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "An unhandled exception occurred while processing the request.");
        }

        var detail = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message;

        await Results.Problem(
                statusCode: statusCode,
                title: title,
                detail: detail,
                instance: httpContext.Request.Path)
            .ExecuteAsync(httpContext);

        return true;
    }
}

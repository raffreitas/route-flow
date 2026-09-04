namespace RouteFlow.Api.Modules.Deliveries;

internal static class DeliveryEndpointConventionExtensions
{
    public static RouteHandlerBuilder ProducesDeliveryCommandResponses(this RouteHandlerBuilder builder)
    {
        return builder
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }
}

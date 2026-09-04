using RouteFlow.Fleet.Application.Exceptions;
using RouteFlow.Fleet.Application.Features.RegisterDriver;
using RouteFlow.Fleet.Application.Features.SetDriverAvailability;
using RouteFlow.Fleet.Contracts.DriverAvailability;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Api.Modules.Fleet;

public static class FleetEndpoints
{
    public static IEndpointRouteBuilder MapFleetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/drivers").WithTags("Fleet");

        group.MapPost(string.Empty, RegisterDriverAsync)
            .WithName("RegisterDriver")
            .Produces<RegisterDriverResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPut("/{driverId:guid}/availability", SetAvailabilityAsync)
            .WithName("SetDriverAvailability")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{driverId:guid}/availability", GetAvailabilityAsync)
            .WithName("GetDriverAvailability")
            .Produces<DriverAvailabilityResult>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> RegisterDriverAsync(
        RegisterDriverRequest request,
        RegisterDriverCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var driverId = await handler.HandleAsync(
            new RegisterDriverCommand(request.Name, request.VehicleType),
            cancellationToken);
        return Results.Created($"/drivers/{driverId.Value}/availability", new RegisterDriverResponse(driverId.Value));
    }

    private static async Task<IResult> SetAvailabilityAsync(
        Guid driverId,
        SetDriverAvailabilityRequest request,
        SetDriverAvailabilityCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new SetDriverAvailabilityCommand(DriverId.From(driverId), request.IsAvailable),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetAvailabilityAsync(
        Guid driverId,
        IDriverAvailabilityReader reader,
        CancellationToken cancellationToken)
    {
        var result = await reader.GetAsync(driverId, cancellationToken)
            ?? throw new DriverNotFoundException(DriverId.From(driverId));
        return Results.Ok(result);
    }
}

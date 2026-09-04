using RouteFlow.Deliveries.Application.Deliveries.AssignDriver;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmArrivalAtPickup;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmPickup;
using RouteFlow.Deliveries.Application.Deliveries.ReleaseDriverBeforePickup;
using RouteFlow.Deliveries.Application.Deliveries.ReportIncompatibleVehicle;
using RouteFlow.Deliveries.Application.Deliveries.StartDispatchToPickup;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Api.Modules.Deliveries;

internal static class DeliveryPickupEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{deliveryId:guid}/assign-driver", AssignDriverAsync)
            .WithName("AssignDeliveryDriver")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/dispatch-to-pickup", StartDispatchToPickupAsync)
            .WithName("StartDeliveryDispatchToPickup")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/arrive-at-pickup", ConfirmArrivalAtPickupAsync)
            .WithName("ConfirmDeliveryArrivalAtPickup")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/pickup", ConfirmPickupAsync)
            .WithName("ConfirmDeliveryPickup")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/release-driver", ReleaseDriverAsync)
            .WithName("ReleaseDeliveryDriver")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/incompatible-vehicle", ReportIncompatibleVehicleAsync)
            .WithName("ReportDeliveryIncompatibleVehicle")
            .ProducesDeliveryCommandResponses();
    }

    private static async Task<IResult> AssignDriverAsync(
        Guid deliveryId,
        AssignDriverRequest request,
        AssignDriverCommandHandler handler,
        CancellationToken cancellationToken)
    {
        VehicleType? vehicleType = null;
        if (request.VehicleType is not null)
        {
            if (!TryParseVehicleType(request.VehicleType, out var parsedVehicleType))
            {
                return InvalidVehicleType(nameof(request.VehicleType));
            }

            vehicleType = parsedVehicleType;
        }

        await handler.HandleAsync(
            new AssignDriverCommand(
                DeliveryId.From(deliveryId),
                DriverId.From(request.DriverId),
                vehicleType),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> StartDispatchToPickupAsync(
        Guid deliveryId,
        StartDispatchToPickupCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new StartDispatchToPickupCommand(DeliveryId.From(deliveryId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmArrivalAtPickupAsync(
        Guid deliveryId,
        ConfirmArrivalAtPickupCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ConfirmArrivalAtPickupCommand(DeliveryId.From(deliveryId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmPickupAsync(
        Guid deliveryId,
        DriverActionRequest request,
        ConfirmPickupCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ConfirmPickupCommand(
                DeliveryId.From(deliveryId),
                DriverId.From(request.DriverId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ReleaseDriverAsync(
        Guid deliveryId,
        ReasonRequest request,
        ReleaseDriverBeforePickupCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ReleaseDriverBeforePickupCommand(
                DeliveryId.From(deliveryId),
                request.Reason),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ReportIncompatibleVehicleAsync(
        Guid deliveryId,
        ReportIncompatibleVehicleRequest request,
        ReportIncompatibleVehicleCommandHandler handler,
        CancellationToken cancellationToken)
    {
        if (!TryParseVehicleType(request.RequiredVehicleType, out var requiredVehicleType))
        {
            return InvalidVehicleType(nameof(request.RequiredVehicleType));
        }

        await handler.HandleAsync(
            new ReportIncompatibleVehicleCommand(
                DeliveryId.From(deliveryId),
                requiredVehicleType,
                request.Reason),
            cancellationToken);
        return Results.NoContent();
    }

    private static bool TryParseVehicleType(string value, out VehicleType vehicleType)
    {
        return Enum.TryParse(value, ignoreCase: true, out vehicleType)
            && Enum.IsDefined(vehicleType);
    }

    private static IResult InvalidVehicleType(string field)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [field] = ["The vehicle type is not supported."]
        });
    }
}

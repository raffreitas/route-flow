using RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;
using RouteFlow.Deliveries.Application.Deliveries.CheckInPackageAtHub;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmAddressChangeInTransit;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmReturnToSender;
using RouteFlow.Deliveries.Application.Deliveries.DispatchToNewRoute;
using RouteFlow.Deliveries.Application.Deliveries.ExpireOperationalIssue;
using RouteFlow.Deliveries.Application.Deliveries.ResolveAddressIssue;
using RouteFlow.Deliveries.Application.Deliveries.UpdateAddressBeforePickup;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Api.Modules.Deliveries;

internal static class DeliveryRecoveryEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{deliveryId:guid}/address", UpdateAddressBeforePickupAsync)
            .WithName("UpdateDeliveryAddressBeforePickup")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/in-transit-address-change", ConfirmAddressChangeInTransitAsync)
            .WithName("ConfirmDeliveryAddressChangeInTransit")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/operational-issue/resolve", ResolveOperationalIssueAsync)
            .WithName("ResolveDeliveryOperationalIssue")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/operational-issue/expire", ExpireOperationalIssueAsync)
            .WithName("ExpireDeliveryOperationalIssue")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/dispatch-to-new-route", DispatchToNewRouteAsync)
            .WithName("DispatchDeliveryToNewRoute")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/hub-check-in", CheckInPackageAtHubAsync)
            .WithName("CheckInDeliveryPackageAtHub")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/return/authorize", AuthorizeReturnAsync)
            .WithName("AuthorizeDeliveryReturn")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/return/confirm", ConfirmReturnToSenderAsync)
            .WithName("ConfirmDeliveryReturnToSender")
            .ProducesDeliveryCommandResponses();
    }

    private static async Task<IResult> UpdateAddressBeforePickupAsync(
        Guid deliveryId,
        UpdateDeliveryAddressRequest request,
        UpdateAddressBeforePickupCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new UpdateAddressBeforePickupCommand(
                DeliveryId.From(deliveryId),
                ToAddress(request)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmAddressChangeInTransitAsync(
        Guid deliveryId,
        UpdateDeliveryAddressRequest request,
        ConfirmAddressChangeInTransitCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ConfirmAddressChangeInTransitCommand(
                DeliveryId.From(deliveryId),
                ToAddress(request)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ResolveOperationalIssueAsync(
        Guid deliveryId,
        UpdateDeliveryAddressRequest request,
        ResolveAddressIssueCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ResolveAddressIssueCommand(
                DeliveryId.From(deliveryId),
                ToAddress(request)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ExpireOperationalIssueAsync(
        Guid deliveryId,
        ExpireOperationalIssueCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ExpireOperationalIssueCommand(DeliveryId.From(deliveryId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DispatchToNewRouteAsync(
        Guid deliveryId,
        DispatchToNewRouteRequest request,
        DispatchToNewRouteCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var driverId = request.DriverId.HasValue
            ? DriverId.From(request.DriverId.Value)
            : (DriverId?)null;
        await handler.HandleAsync(
            new DispatchToNewRouteCommand(DeliveryId.From(deliveryId), driverId),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CheckInPackageAtHubAsync(
        Guid deliveryId,
        CheckInPackageAtHubRequest request,
        CheckInPackageAtHubCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new CheckInPackageAtHubCommand(
                DeliveryId.From(deliveryId),
                HubId.From(request.HubId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AuthorizeReturnAsync(
        Guid deliveryId,
        ReasonRequest request,
        AuthorizeReturnCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AuthorizeReturnCommand(DeliveryId.From(deliveryId), request.Reason),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmReturnToSenderAsync(
        Guid deliveryId,
        ConfirmReturnToSenderCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ConfirmReturnToSenderCommand(DeliveryId.From(deliveryId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static DeliveryAddress ToAddress(UpdateDeliveryAddressRequest request)
    {
        return new DeliveryAddress(
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State,
            request.ZipCode);
    }
}

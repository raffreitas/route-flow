using RouteFlow.Deliveries.Application.Deliveries.CancelDelivery;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmDeliveryToRecipient;
using RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;
using RouteFlow.Deliveries.Application.Deliveries.ReportTransitIncident;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Api.Modules.Deliveries;

internal static class DeliveryLifecycleEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/{deliveryId:guid}/complete", CompleteDeliveryAsync)
            .WithName("CompleteDelivery")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/failed-attempts", RecordFailedAttemptAsync)
            .WithName("RecordDeliveryFailedAttempt")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/cancel", CancelDeliveryAsync)
            .WithName("CancelDelivery")
            .ProducesDeliveryCommandResponses();

        group.MapPost("/{deliveryId:guid}/transit-incidents", ReportTransitIncidentAsync)
            .WithName("ReportDeliveryTransitIncident")
            .ProducesDeliveryCommandResponses();
    }

    private static async Task<IResult> CompleteDeliveryAsync(
        Guid deliveryId,
        ConfirmDeliveryToRecipientCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ConfirmDeliveryToRecipientCommand(DeliveryId.From(deliveryId)),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RecordFailedAttemptAsync(
        Guid deliveryId,
        ReportFailedAttemptRequest request,
        RecordFailedAttemptCommandHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<FailureCategory>(request.Category, ignoreCase: true, out var category)
            || !Enum.IsDefined(category))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.Category)] = ["The failure category is not supported."]
            });
        }

        await handler.HandleAsync(
            new RecordFailedAttemptCommand(
                DeliveryId.From(deliveryId),
                new FailureReason(category, request.Description),
                request.Notes),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelDeliveryAsync(
        Guid deliveryId,
        ReasonRequest request,
        CancelDeliveryCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new CancelDeliveryCommand(DeliveryId.From(deliveryId), request.Reason),
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ReportTransitIncidentAsync(
        Guid deliveryId,
        ReasonRequest request,
        ReportTransitIncidentCommandHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ReportTransitIncidentCommand(DeliveryId.From(deliveryId), request.Reason),
            cancellationToken);
        return Results.NoContent();
    }
}

using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Deliveries.GetDelivery;
using RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Api.Modules.Deliveries;

public static class DeliveriesEndpoints
{
    public static IEndpointRouteBuilder MapDeliveriesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/deliveries").WithTags("Deliveries");

        group.MapPost(string.Empty, CreateDeliveryAsync)
            .WithName("CreateDelivery")
            .Produces<CreateDeliveryResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{deliveryId:guid}", GetDeliveryAsync)
            .WithName("GetDelivery")
            .Produces<DeliveryDetails>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        DeliveryPickupEndpoints.Map(group);
        DeliveryLifecycleEndpoints.Map(group);
        DeliveryRecoveryEndpoints.Map(group);

        return endpoints;
    }

    private static async Task<IResult> CreateDeliveryAsync(
        CreateDeliveryRequest request,
        RequestDeliveryCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RequestDeliveryCommand(
            request.MerchantId,
            new DeliveryAddressInput(
                request.Address.Street,
                request.Address.Number,
                request.Address.Complement,
                request.Address.Neighborhood,
                request.Address.City,
                request.Address.State,
                request.Address.ZipCode),
            new PackageInput(
                request.Package.WeightKg,
                request.Package.LengthCm,
                request.Package.WidthCm,
                request.Package.HeightCm,
                request.Package.Description));
        var deliveryId = await handler.HandleAsync(command, cancellationToken);

        return Results.Created(
            $"/deliveries/{deliveryId.Value}",
            new CreateDeliveryResponse(deliveryId.Value));
    }

    private static async Task<IResult> GetDeliveryAsync(
        Guid deliveryId,
        IDeliveryQueries queries,
        CancellationToken cancellationToken)
    {
        var id = DeliveryId.From(deliveryId);
        var delivery = await queries.GetByIdAsync(id, cancellationToken)
            ?? throw new DeliveryNotFoundException(id);

        return Results.Ok(delivery);
    }

}

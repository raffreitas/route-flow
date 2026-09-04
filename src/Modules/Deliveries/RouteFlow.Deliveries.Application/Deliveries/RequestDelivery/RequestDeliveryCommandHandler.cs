using FluentValidation;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;

public sealed class RequestDeliveryCommandHandler(
    IDeliveryRepository repository,
    TimeProvider timeProvider,
    IValidator<RequestDeliveryCommand>? validator = null)
{
    private readonly IValidator<RequestDeliveryCommand> _validator = validator ?? new RequestDeliveryCommandValidator();

    public async Task<DeliveryId> HandleAsync(
        RequestDeliveryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var address = new DeliveryAddress(
            command.Address.Street,
            command.Address.Number,
            command.Address.Complement,
            command.Address.Neighborhood,
            command.Address.City,
            command.Address.State,
            command.Address.ZipCode);

        var package = new PackageInfo(
            command.Package.WeightKg,
            new PackageDimensions(
                command.Package.LengthCm,
                command.Package.WidthCm,
                command.Package.HeightCm),
            command.Package.Description);

        var delivery = Delivery.Request(
            DeliveryId.New(),
            MerchantId.From(command.MerchantId),
            address,
            package,
            timeProvider.GetUtcNow());

        await repository.AddAsync(delivery, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return delivery.Id;
    }
}

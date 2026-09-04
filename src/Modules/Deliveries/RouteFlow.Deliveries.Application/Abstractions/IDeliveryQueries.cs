using RouteFlow.Deliveries.Application.Deliveries.GetDelivery;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Abstractions;

public interface IDeliveryQueries
{
    Task<DeliveryDetails?> GetByIdAsync(
        DeliveryId deliveryId,
        CancellationToken cancellationToken = default);
}

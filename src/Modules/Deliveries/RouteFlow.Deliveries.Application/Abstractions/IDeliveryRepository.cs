using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.Abstractions;

public interface IDeliveryRepository
{
    Task<Delivery?> GetByIdAsync(DeliveryId deliveryId, CancellationToken cancellationToken = default);
    Task AddAsync(Delivery delivery, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

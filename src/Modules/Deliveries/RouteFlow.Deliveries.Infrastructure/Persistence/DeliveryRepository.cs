using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Infrastructure.Persistence;

internal sealed class DeliveryRepository(DeliveriesDbContext dbContext) : IDeliveryRepository
{
    public Task<Delivery?> GetByIdAsync(
        DeliveryId deliveryId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Deliveries.SingleOrDefaultAsync(
            delivery => delivery.Id == deliveryId,
            cancellationToken);
    }

    public async Task AddAsync(Delivery delivery, CancellationToken cancellationToken = default)
    {
        await dbContext.Deliveries.AddAsync(delivery, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

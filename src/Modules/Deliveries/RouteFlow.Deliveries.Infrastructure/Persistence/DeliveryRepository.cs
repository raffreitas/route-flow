using Microsoft.EntityFrameworkCore;
using RouteFlow.Deliveries.Application.Abstractions;
using RouteFlow.Deliveries.Application.Exceptions;
using RouteFlow.Deliveries.Domain;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Infrastructure.Persistence;

internal sealed class DeliveryRepository(DeliveriesDbContext dbContext) : IDeliveryRepository
{
    public Task<Delivery?> GetByIdAsync(
        DeliveryId deliveryId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Deliveries
            .Include(delivery => delivery.Attempts)
            .SingleOrDefaultAsync(
                delivery => delivery.Id == deliveryId,
                cancellationToken);
    }

    public async Task AddAsync(Delivery delivery, CancellationToken cancellationToken = default)
    {
        await dbContext.Deliveries.AddAsync(delivery, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var delivery = exception.Entries
                .Select(entry => entry.Entity)
                .OfType<Delivery>()
                .Single();

            throw new DeliveryConcurrencyException(delivery.Id, exception);
        }
    }
}

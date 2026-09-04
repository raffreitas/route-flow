using Microsoft.EntityFrameworkCore;
using RouteFlow.Fleet.Application.Abstractions;
using RouteFlow.Fleet.Application.Exceptions;
using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Infrastructure.Persistence;

internal sealed class DriverRepository(FleetDbContext dbContext) : IDriverRepository
{
    public Task<Driver?> GetByIdAsync(DriverId driverId, CancellationToken cancellationToken = default)
    {
        return dbContext.Drivers.SingleOrDefaultAsync(driver => driver.Id == driverId, cancellationToken);
    }

    public async Task AddAsync(Driver driver, CancellationToken cancellationToken = default)
    {
        await dbContext.Drivers.AddAsync(driver, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var driver = exception.Entries.Select(entry => entry.Entity).OfType<Driver>().Single();
            throw new DriverConcurrencyException(driver.Id, exception);
        }
    }
}

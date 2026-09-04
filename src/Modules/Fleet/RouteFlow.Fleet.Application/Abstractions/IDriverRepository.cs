using RouteFlow.Fleet.Domain;
using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.Abstractions;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(DriverId driverId, CancellationToken cancellationToken = default);
    Task AddAsync(Driver driver, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

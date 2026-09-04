namespace RouteFlow.Fleet.Contracts.DriverAvailability;

public interface IDriverAvailabilityReader
{
    Task<DriverAvailabilityResult?> GetAsync(
        Guid driverId,
        CancellationToken cancellationToken = default);
}

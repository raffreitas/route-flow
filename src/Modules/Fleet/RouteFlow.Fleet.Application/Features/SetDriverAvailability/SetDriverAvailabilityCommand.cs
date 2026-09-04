using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.Features.SetDriverAvailability;

public sealed record SetDriverAvailabilityCommand(DriverId DriverId, bool IsAvailable);

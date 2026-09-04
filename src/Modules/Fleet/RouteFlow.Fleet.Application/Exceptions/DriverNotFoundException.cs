using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.Exceptions;

public sealed class DriverNotFoundException(DriverId driverId)
    : Exception($"Driver '{driverId}' was not found.");

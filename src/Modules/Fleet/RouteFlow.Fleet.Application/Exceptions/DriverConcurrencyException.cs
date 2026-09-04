using RouteFlow.Fleet.Domain.ValueObjects;

namespace RouteFlow.Fleet.Application.Exceptions;

public sealed class DriverConcurrencyException(DriverId driverId, Exception innerException)
    : Exception($"Driver '{driverId}' was updated by another operation.", innerException);

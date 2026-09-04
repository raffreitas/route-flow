namespace RouteFlow.Api.Modules.Deliveries;

public sealed record CheckInPackageAtHubRequest([NotEmptyGuid] Guid HubId);

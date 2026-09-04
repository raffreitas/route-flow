using Microsoft.Extensions.DependencyInjection;
using RouteFlow.Deliveries.Application.Deliveries.AssignDriver;
using RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;
using RouteFlow.Deliveries.Application.Deliveries.CancelDelivery;
using RouteFlow.Deliveries.Application.Deliveries.CheckInPackageAtHub;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmAddressChangeInTransit;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmArrivalAtPickup;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmDeliveryToRecipient;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmPickup;
using RouteFlow.Deliveries.Application.Deliveries.ConfirmReturnToSender;
using RouteFlow.Deliveries.Application.Deliveries.DispatchToNewRoute;
using RouteFlow.Deliveries.Application.Deliveries.ExpireOperationalIssue;
using RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;
using RouteFlow.Deliveries.Application.Deliveries.ReleaseDriverBeforePickup;
using RouteFlow.Deliveries.Application.Deliveries.ReportIncompatibleVehicle;
using RouteFlow.Deliveries.Application.Deliveries.ReportTransitIncident;
using RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;
using RouteFlow.Deliveries.Application.Deliveries.ResolveAddressIssue;
using RouteFlow.Deliveries.Application.Deliveries.StartDispatchToPickup;
using RouteFlow.Deliveries.Application.Deliveries.UpdateAddressBeforePickup;

namespace RouteFlow.Deliveries.Application.UnitTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddDeliveriesApplication_WhenCalled_ShouldRegisterAllCommandHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDeliveriesApplication();

        // Assert
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TimeProvider));
        AssertScoped<AssignDriverCommandHandler>(services);
        AssertScoped<AuthorizeReturnCommandHandler>(services);
        AssertScoped<CancelDeliveryCommandHandler>(services);
        AssertScoped<CheckInPackageAtHubCommandHandler>(services);
        AssertScoped<ConfirmAddressChangeInTransitCommandHandler>(services);
        AssertScoped<ConfirmArrivalAtPickupCommandHandler>(services);
        AssertScoped<ConfirmDeliveryToRecipientCommandHandler>(services);
        AssertScoped<ConfirmPickupCommandHandler>(services);
        AssertScoped<ConfirmReturnToSenderCommandHandler>(services);
        AssertScoped<DispatchToNewRouteCommandHandler>(services);
        AssertScoped<ExpireOperationalIssueCommandHandler>(services);
        AssertScoped<RecordFailedAttemptCommandHandler>(services);
        AssertScoped<ReleaseDriverBeforePickupCommandHandler>(services);
        AssertScoped<ReportIncompatibleVehicleCommandHandler>(services);
        AssertScoped<ReportTransitIncidentCommandHandler>(services);
        AssertScoped<RequestDeliveryCommandHandler>(services);
        AssertScoped<ResolveAddressIssueCommandHandler>(services);
        AssertScoped<StartDispatchToPickupCommandHandler>(services);
        AssertScoped<UpdateAddressBeforePickupCommandHandler>(services);
    }

    private static void AssertScoped<THandler>(IEnumerable<ServiceDescriptor> services)
    {
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(THandler)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
    }
}

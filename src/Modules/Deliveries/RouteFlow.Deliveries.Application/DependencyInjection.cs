using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

namespace RouteFlow.Deliveries.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDeliveriesApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IValidator<AuthorizeReturnCommand>, AuthorizeReturnCommandValidator>();
        services.AddScoped<IValidator<CancelDeliveryCommand>, CancelDeliveryCommandValidator>();
        services.AddScoped<IValidator<ConfirmAddressChangeInTransitCommand>, ConfirmAddressChangeInTransitCommandValidator>();
        services.AddScoped<IValidator<RecordFailedAttemptCommand>, RecordFailedAttemptCommandValidator>();
        services.AddScoped<IValidator<ReleaseDriverBeforePickupCommand>, ReleaseDriverBeforePickupCommandValidator>();
        services.AddScoped<IValidator<ReportIncompatibleVehicleCommand>, ReportIncompatibleVehicleCommandValidator>();
        services.AddScoped<IValidator<ReportTransitIncidentCommand>, ReportTransitIncidentCommandValidator>();
        services.AddScoped<IValidator<RequestDeliveryCommand>, RequestDeliveryCommandValidator>();
        services.AddScoped<IValidator<ResolveAddressIssueCommand>, ResolveAddressIssueCommandValidator>();
        services.AddScoped<IValidator<UpdateAddressBeforePickupCommand>, UpdateAddressBeforePickupCommandValidator>();
        services.AddScoped<AssignDriverCommandHandler>();
        services.AddScoped<AuthorizeReturnCommandHandler>();
        services.AddScoped<CancelDeliveryCommandHandler>();
        services.AddScoped<CheckInPackageAtHubCommandHandler>();
        services.AddScoped<ConfirmAddressChangeInTransitCommandHandler>();
        services.AddScoped<ConfirmArrivalAtPickupCommandHandler>();
        services.AddScoped<ConfirmDeliveryToRecipientCommandHandler>();
        services.AddScoped<ConfirmPickupCommandHandler>();
        services.AddScoped<ConfirmReturnToSenderCommandHandler>();
        services.AddScoped<DispatchToNewRouteCommandHandler>();
        services.AddScoped<ExpireOperationalIssueCommandHandler>();
        services.AddScoped<RecordFailedAttemptCommandHandler>();
        services.AddScoped<ReleaseDriverBeforePickupCommandHandler>();
        services.AddScoped<ReportIncompatibleVehicleCommandHandler>();
        services.AddScoped<ReportTransitIncidentCommandHandler>();
        services.AddScoped<RequestDeliveryCommandHandler>();
        services.AddScoped<ResolveAddressIssueCommandHandler>();
        services.AddScoped<StartDispatchToPickupCommandHandler>();
        services.AddScoped<UpdateAddressBeforePickupCommandHandler>();

        return services;
    }
}

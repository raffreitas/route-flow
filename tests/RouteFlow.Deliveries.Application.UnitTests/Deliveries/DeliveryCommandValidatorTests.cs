using FluentValidation;
using RouteFlow.Deliveries.Application.Deliveries.AuthorizeReturn;
using RouteFlow.Deliveries.Application.Deliveries.CancelDelivery;
using RouteFlow.Deliveries.Application.Deliveries.RecordFailedAttempt;
using RouteFlow.Deliveries.Application.Deliveries.ReleaseDriverBeforePickup;
using RouteFlow.Deliveries.Application.Deliveries.ReportIncompatibleVehicle;
using RouteFlow.Deliveries.Application.Deliveries.ReportTransitIncident;
using RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;
using RouteFlow.Deliveries.Application.Deliveries.UpdateAddressBeforePickup;
using RouteFlow.Deliveries.Application.UnitTests.TestSupport;
using RouteFlow.Deliveries.Application.Validation;
using RouteFlow.Deliveries.Domain.Enums;
using RouteFlow.Deliveries.Domain.ValueObjects;

namespace RouteFlow.Deliveries.Application.UnitTests.Deliveries;

public sealed class DeliveryCommandValidatorTests
{
    [Fact]
    public async Task Validate_RequestWithInvalidInput_ShouldReturnNestedValidationErrors()
    {
        var command = new RequestDeliveryCommand(
            Guid.Empty,
            new DeliveryAddressInput("", "1", null, "Neighborhood", "City", "S", "Zip"),
            new PackageInput(0, 0, 1, 1, ""));

        var result = await new RequestDeliveryCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(command.MerchantId));
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Address.Street");
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Address.State");
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Package.WeightKg");
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Package.LengthCm");
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Package.Description");
    }

    [Fact]
    public async Task Validate_AddressCommandWithInvalidAddress_ShouldReturnAddressErrors()
    {
        var invalidAddress = DeliveryTestData.CreateAddress() with
        {
            Street = "",
            State = "São Paulo"
        };
        var command = new UpdateAddressBeforePickupCommand(DeliveryId.New(), invalidAddress);

        var result = await new UpdateAddressBeforePickupCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == "NewAddress.Street");
        Assert.Contains(result.Errors, failure => failure.PropertyName == "NewAddress.State");
    }

    [Fact]
    public async Task Validate_ReasonCommandsWithBlankReason_ShouldRejectAllCommands()
    {
        var deliveryId = DeliveryId.New();

        await AssertInvalidAsync(
            new AuthorizeReturnCommandValidator(),
            new AuthorizeReturnCommand(deliveryId, " "));
        await AssertInvalidAsync(
            new CancelDeliveryCommandValidator(),
            new CancelDeliveryCommand(deliveryId, " "));
        await AssertInvalidAsync(
            new ReleaseDriverBeforePickupCommandValidator(),
            new ReleaseDriverBeforePickupCommand(deliveryId, " "));
        await AssertInvalidAsync(
            new ReportTransitIncidentCommandValidator(),
            new ReportTransitIncidentCommand(deliveryId, " "));
    }

    [Fact]
    public async Task Validate_FailedAttemptWithInvalidDetails_ShouldReturnValidationErrors()
    {
        var command = new RecordFailedAttemptCommand(
            DeliveryId.New(),
            new FailureReason((FailureCategory)999, ""),
            new string('n', DeliveryValidationLimits.NotesMaxLength + 1));

        var result = await new RecordFailedAttemptCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Reason.Category");
        Assert.Contains(result.Errors, failure => failure.PropertyName == "Reason.Description");
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(command.Notes));
    }

    [Fact]
    public async Task Validate_IncompatibleVehicleWithInvalidInput_ShouldReturnValidationErrors()
    {
        var command = new ReportIncompatibleVehicleCommand(
            DeliveryId.New(),
            (VehicleType)999,
            "");

        var result = await new ReportIncompatibleVehicleCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(command.RequiredVehicleType));
        Assert.Contains(result.Errors, failure => failure.PropertyName == nameof(command.Reason));
    }

    private static async Task AssertInvalidAsync<TCommand>(IValidator<TCommand> validator, TCommand command)
    {
        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }
}

using FluentValidation;

namespace RouteFlow.Deliveries.Application.Deliveries.RequestDelivery;

public sealed class RequestDeliveryCommandValidator : AbstractValidator<RequestDeliveryCommand>
{
    public RequestDeliveryCommandValidator()
    {
        RuleFor(command => command.MerchantId).NotEmpty();
        RuleFor(command => command.Address)
            .NotNull()
            .SetValidator(new DeliveryAddressInputValidator());
        RuleFor(command => command.Package)
            .NotNull()
            .SetValidator(new PackageInputValidator());
    }
}

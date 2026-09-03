using RouteFlow.Deliveries.Domain.Enums;

namespace RouteFlow.Deliveries.Domain.ValueObjects;

public sealed record FailureReason(FailureCategory Category, string Description)
{
    public bool AllowsImmediateRetry => Category switch
    {
        FailureCategory.RecipientAbsent => true,
        _ => false
    };

    public bool RequiresOperationalIssueQueue => Category switch
    {
        FailureCategory.AddressNotFound => true,
        FailureCategory.AccessRestricted => true,
        _ => false
    };
}
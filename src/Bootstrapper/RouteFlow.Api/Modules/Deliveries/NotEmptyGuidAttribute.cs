using System.ComponentModel.DataAnnotations;

namespace RouteFlow.Api.Modules.Deliveries;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is null || value is Guid guid && guid != Guid.Empty;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"The {name} field must not be an empty GUID.";
    }
}

using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.AuthorizeAccess.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class EmailAddressAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is string str && EmailAddress.TryParse(str, out _);
    }
}

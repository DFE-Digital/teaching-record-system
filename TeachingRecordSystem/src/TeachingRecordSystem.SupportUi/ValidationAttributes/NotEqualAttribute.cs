namespace TeachingRecordSystem.SupportUi.ValidationAttributes;

using System.ComponentModel.DataAnnotations;

public class NotEqualAttribute : ValidationAttribute
{
    private readonly object _notAllowedValue;

    public NotEqualAttribute(object notAllowedValue)
    {
        _notAllowedValue = notAllowedValue;
    }

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        if (value != null && value.Equals(_notAllowedValue))
        {
            return new ValidationResult(ErrorMessage ?? $"The value cannot be {_notAllowedValue}.");
        }

        return ValidationResult.Success;
    }
}

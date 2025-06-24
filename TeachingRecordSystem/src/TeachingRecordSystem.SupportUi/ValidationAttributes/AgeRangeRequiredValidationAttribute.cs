using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.ValidationAttributes;

public class AgeRangeRequiredValidationAttribute() : ValidationAttribute()
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not AgeRange ageRange)
        {
            throw new InvalidOperationException($"The {nameof(AgeRangeValidationAttribute)} must be applied to {nameof(AgeRange)} types.");
        }
        else if (ageRange.AgeRangeType is null)
        {
            return new ValidationResult("Enter an age range specialism or select 'Not provided'", new List<string> { nameof(ageRange.AgeRangeType) });
        }

        return ValidationResult.Success;
    }
}

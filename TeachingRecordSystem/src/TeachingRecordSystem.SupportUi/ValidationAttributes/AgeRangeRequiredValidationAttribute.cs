using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.ValidationAttributes;

public class AgeRangeRequiredValidationAttribute(string errorMessage) : ValidationAttribute(errorMessage)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not AgeRange ageRange)
        {
            throw new InvalidOperationException($"The {nameof(AgeRangeValidationAttribute)} must be applied to {nameof(AgeRange)} types.");
        }
        else if (ageRange.AgeRangeType is null)
        {
            return new ValidationResult("Select an age range", new List<string> { nameof(ageRange.AgeRangeType) });
        }

        return ValidationResult.Success;
    }
}

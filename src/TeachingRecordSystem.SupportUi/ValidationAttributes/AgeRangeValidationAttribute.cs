using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.ValidationAttributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AgeRangeValidationAttribute(string errorMessage) : ValidationAttribute(errorMessage)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not AgeRange ageRange)
        {
            throw new InvalidOperationException($"The {nameof(AgeRangeValidationAttribute)} must be applied to {nameof(AgeRange)} types.");
        }

        if (ageRange.AgeRangeType == TrainingAgeSpecialismType.Range)
        {
            if (ageRange.AgeRangeFrom != null && ageRange.AgeRangeTo != null && ageRange.AgeRangeFrom > ageRange.AgeRangeTo)
            {
                return new ValidationResult("The 'from' age must be less than or equal to the 'to' age", new List<string> { nameof(ageRange.AgeRangeFrom) });
            }
        }
        else
        {
            // Clear any range values if an age range type has been selected
            ageRange.AgeRangeFrom = null;
            ageRange.AgeRangeTo = null;
        }

        return ValidationResult.Success;
    }
}

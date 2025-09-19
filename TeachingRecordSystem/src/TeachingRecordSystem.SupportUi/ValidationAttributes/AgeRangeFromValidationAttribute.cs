using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.ValidationAttributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AgeRangeFromValidationAttribute(string errorMessage) : ValidationAttribute(errorMessage)
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not AgeRange ageRange)
        {
            throw new InvalidOperationException($"The {nameof(AgeRangeValidationAttribute)} must be applied to {nameof(AgeRange)} types.");
        }

        if (ageRange.AgeRangeType == TrainingAgeSpecialismType.Range)
        {
            if (ageRange.AgeRangeFrom == null)
            {
                return new ValidationResult("Enter a 'from' age", new List<string> { nameof(ageRange.AgeRangeFrom) });
            }
            else if (ageRange.AgeRangeFrom < 0 || ageRange.AgeRangeFrom > 20)
            {
                return new ValidationResult($"Age must be within the range 0 to 20", new List<string> { nameof(ageRange.AgeRangeFrom) });
            }
        }

        return ValidationResult.Success;

    }
}

using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.ValidationAttributes;

public class AgeRangeValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not AgeRange ageRange)
        {
            return new ValidationResult("The value must be of type AgeRange.");
        }
        if (ageRange.AgeRangeType is null || ageRange.AgeRangeType == TrainingAgeSpecialismType.None)
        {
            if (ageRange.AgeRangeFrom == null || ageRange.AgeRangeTo == null)
            {
                return new ValidationResult("Both range values must be provided.");
            }
            if (ageRange.AgeRangeFrom < 0 || ageRange.AgeRangeTo < 0)
            {
                return new ValidationResult("Both range values must be positive.");
            }
            if (ageRange.AgeRangeFrom > ageRange.AgeRangeTo)
            {
                return new ValidationResult("The 'from' value must be less than or equal to the 'to' value.");
            }
        }
        else
        {
            if (ageRange.AgeRangeFrom != null || ageRange.AgeRangeTo != null)
            {
                return new ValidationResult("Both range values must be null.");
            }
        }
        return ValidationResult.Success;
    }
}

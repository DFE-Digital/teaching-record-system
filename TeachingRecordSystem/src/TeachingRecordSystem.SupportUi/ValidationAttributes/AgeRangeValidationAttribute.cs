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
            throw new InvalidOperationException($"The {nameof(AgeRangeValidationAttribute)} must be appplied to {nameof(AgeRange)} types.");
        }
        if (ageRange.AgeRangeType is null || ageRange.AgeRangeType == TrainingAgeSpecialismType.None)
        {
            if (ageRange.AgeRangeFrom == null)
            {
                return new ValidationResult("Age value must be provided.", new List<string> { nameof(ageRange.AgeRangeFrom) });
            }
            if (ageRange.AgeRangeTo == null)
            {
                return new ValidationResult("Age value must be provided.", new List<string> { nameof(ageRange.AgeRangeTo) });
            }
            if (ageRange.AgeRangeFrom > ageRange.AgeRangeTo)
            {
                return new ValidationResult("The 'from' value must be less than or equal to the 'to' value.", new List<string> { nameof(ageRange.AgeRangeFrom) });
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

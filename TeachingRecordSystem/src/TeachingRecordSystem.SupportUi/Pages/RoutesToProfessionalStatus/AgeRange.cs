using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

[AgeRangeValidation("Enter a valid age range specialism")]
[AgeRangeFromValidation("Enter a valid age range specialism")]
[AgeRangeToValidation("Enter a valid age range specialism")]
public class AgeRange
{
    [Display(Name = "Edit age range specialism")]
    public TrainingAgeSpecialismType? AgeRangeType { get; set; }

    [Display(Name = "From")]
    [Range(0, 20, ErrorMessage = "Age must be within the range {1} to {2}")]
    public int? AgeRangeFrom { get; set; }

    [Display(Name = "To")]
    [Range(0, 20, ErrorMessage = "Age must be within the range {1} to {2}")]
    public int? AgeRangeTo { get; set; }
}

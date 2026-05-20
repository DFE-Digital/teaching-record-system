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
    public int? AgeRangeFrom { get; set; }

    [Display(Name = "To")]
    public int? AgeRangeTo { get; set; }
}

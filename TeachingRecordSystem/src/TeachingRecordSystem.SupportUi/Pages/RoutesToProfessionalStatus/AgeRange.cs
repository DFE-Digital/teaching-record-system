using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;

public enum AgeSpecializationOption
{
    [Description("Age range")]
    [Display(Name = "Age range")]
    Range = 0,
    [Description("Foundation stage")]
    [Display(Name = "Foundation stage")]
    FoundationStage = 1,
    [Description("Further education")]
    [Display(Name = "Further education")]
    FurtherEducation = 2,
    [Description("Key stage 1")]
    [Display(Name = "Key stage 1")]
    KeyStage1 = 3,
    [Description("Key stage 2")]
    [Display(Name = "Key stage 2")]
    KeyStage2 = 4,
    [Description("Key stage 3")]
    [Display(Name = "Key stage 3")]
    KeyStage3 = 5,
    [Description("Key stage 4")]
    [Display(Name = "Key stage 4")]
    KeyStage4 = 6,
    [Description("None")]
    [Display(Name = "Not provided")]
    None = 7
}

public static class AgeSpecialisationOptionsExtension
{
    public static TrainingAgeSpecialismType? ToTrainingAgeSpecialismType(this AgeSpecializationOption? ageOption)
    {
        return ageOption switch
        {
            null => null,
            AgeSpecializationOption.FoundationStage => TrainingAgeSpecialismType.FoundationStage,
            AgeSpecializationOption.FurtherEducation => TrainingAgeSpecialismType.FurtherEducation,
            AgeSpecializationOption.KeyStage1 => TrainingAgeSpecialismType.KeyStage1,
            AgeSpecializationOption.KeyStage2 => TrainingAgeSpecialismType.KeyStage2,
            AgeSpecializationOption.KeyStage3 => TrainingAgeSpecialismType.KeyStage3,
            AgeSpecializationOption.KeyStage4 => TrainingAgeSpecialismType.KeyStage4,
            AgeSpecializationOption.Range => TrainingAgeSpecialismType.Range,
            AgeSpecializationOption.None => null,
            _ => null
        };
    }

    public static AgeSpecializationOption? ToAgeSpecializationOption(this TrainingAgeSpecialismType? ageOption)
    {
        return ageOption switch
        {
            null => null,
            TrainingAgeSpecialismType.FoundationStage => AgeSpecializationOption.FoundationStage,
            TrainingAgeSpecialismType.FurtherEducation => AgeSpecializationOption.FurtherEducation,
            TrainingAgeSpecialismType.KeyStage1 => AgeSpecializationOption.KeyStage1,
            TrainingAgeSpecialismType.KeyStage2 => AgeSpecializationOption.KeyStage2,
            TrainingAgeSpecialismType.KeyStage3 => AgeSpecializationOption.KeyStage3,
            TrainingAgeSpecialismType.KeyStage4 => AgeSpecializationOption.KeyStage4,
            TrainingAgeSpecialismType.Range => AgeSpecializationOption.Range,
            _ => null
        };
    }
}

[AgeRangeValidation("Enter a valid age range specialism")]
public class AgeRange
{
    [Display(Name = "Edit age range specialism")]
    public AgeSpecializationOption? AgeRangeType { get; set; }

    [Display(Name = "From")]
    [Range(0, 20, ErrorMessage = "Age must be within the range {1} to {2}")]
    public int? AgeRangeFrom { get; set; }

    [Display(Name = "To")]
    [Range(0, 20, ErrorMessage = "Age must be within the range {1} to {2}")]
    public int? AgeRangeTo { get; set; }
}

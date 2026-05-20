using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Models;

public enum TrainingAgeSpecialismType
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
    KeyStage4 = 6
}

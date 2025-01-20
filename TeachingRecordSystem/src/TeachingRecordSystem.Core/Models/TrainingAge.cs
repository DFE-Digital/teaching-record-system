using System.ComponentModel;

namespace TeachingRecordSystem.Core.Models;

public enum TrainingAge
{
    [Description("Age range")]
    Range = 0,
    [Description("Foundation stage")]
    FoundationStage = 1,
    [Description("Further education")]
    FurtherEducation = 2,
    [Description("Key stage 1")]
    KeyStage1 = 3,
    [Description("Key stage 2")]
    KeyStage2 = 4,
    [Description("Key stage 3")]
    KeyStage3 = 5,
    [Description("Key stage 4")]
    KeyStage4 = 6
}

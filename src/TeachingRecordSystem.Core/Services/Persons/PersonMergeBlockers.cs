namespace TeachingRecordSystem.Core.Services.Persons;

[Flags]
public enum PersonMergeBlockers
{
    None = 0,
    Deactivated = 1 << 0,
    OpenAlert = 1 << 1,
    InductionStatus = 1 << 2
}

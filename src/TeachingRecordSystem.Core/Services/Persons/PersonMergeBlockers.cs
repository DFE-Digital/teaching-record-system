namespace TeachingRecordSystem.Core.Services.Persons;

[Flags]
public enum PersonMergeBlockers
{
    None = 0,
    Deactivated = 1 << 0,
    OpenAlert = 1 << 1,
    InductionStatusFailedInWales = 1 << 2,
    InductionStatusFailed = 1 << 3,
    InductionStartDateMismatch = 1 << 4,
    InductionCompletedDateMismatch = 1 << 5
}

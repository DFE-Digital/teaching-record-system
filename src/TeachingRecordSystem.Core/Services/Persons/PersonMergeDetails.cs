namespace TeachingRecordSystem.Core.Services.Persons;

public record PersonMergeDetails(
    PersonStatus Status,
    InductionStatus InductionStatus,
    InductionStatus InductionStatusWithoutExemption,
    DateOnly? InductionStartDate,
    DateOnly? InductionCompletedDate,
    bool HasOpenAlert);

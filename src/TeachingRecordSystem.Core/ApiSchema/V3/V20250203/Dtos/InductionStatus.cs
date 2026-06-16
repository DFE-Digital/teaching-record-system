namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

public enum InductionStatus
{
    None = 0,
    RequiredToComplete = 1,
    Exempt = 2,
    InProgress = 3,
    Passed = 4,
    Failed = 5,
    FailedInWales = 6
}

public static class InductionStatusExtensions
{
    extension(InductionStatus)
    {
        public static InductionStatus Create(Models.InductionStatus source) => source switch
        {
            Models.InductionStatus.None => InductionStatus.None,
            Models.InductionStatus.RequiredToComplete => InductionStatus.RequiredToComplete,
            Models.InductionStatus.Exempt => InductionStatus.Exempt,
            Models.InductionStatus.InProgress => InductionStatus.InProgress,
            Models.InductionStatus.Passed => InductionStatus.Passed,
            Models.InductionStatus.Failed => InductionStatus.Failed,
            Models.InductionStatus.FailedInWales => InductionStatus.FailedInWales,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}

using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public enum DqtInductionStatus
{
    Exempt = 1,
    Fail = 2,
    FailedInWales = 3,
    InductionExtended = 4,
    InProgress = 5,
    NotYetCompleted = 6,
    Pass = 7,
    PassedInWales = 8,
    RequiredToComplete = 9
}

public static class DqtInductionStatusExtensions
{
    public static DqtInductionStatus ConvertToDqtInductionStatus(this dfeta_InductionStatus input) => input switch
    {
        dfeta_InductionStatus.Exempt => DqtInductionStatus.Exempt,
        dfeta_InductionStatus.Fail => DqtInductionStatus.Fail,
        dfeta_InductionStatus.FailedinWales => DqtInductionStatus.FailedInWales,
        dfeta_InductionStatus.InductionExtended => DqtInductionStatus.InductionExtended,
        dfeta_InductionStatus.InProgress => DqtInductionStatus.InProgress,
        dfeta_InductionStatus.NotYetCompleted => DqtInductionStatus.NotYetCompleted,
        dfeta_InductionStatus.Pass => DqtInductionStatus.Pass,
        dfeta_InductionStatus.PassedinWales => DqtInductionStatus.PassedInWales,
        dfeta_InductionStatus.RequiredtoComplete => DqtInductionStatus.RequiredToComplete,
        _ => throw new ArgumentException($"Unknown {nameof(DqtInductionStatus)}: '{input}'.")
    };

    public static string GetDescription(this dfeta_InductionStatus input) => ConvertToDqtInductionStatus(input).GetDescription();

    public static string GetDescription(this DqtInductionStatus input) => input switch
    {
        DqtInductionStatus.Exempt => "Exempt",
        DqtInductionStatus.Fail => "Fail",
        DqtInductionStatus.FailedInWales => "Failed in Wales",
        DqtInductionStatus.InductionExtended => "Extended",
        DqtInductionStatus.InProgress => "In progress",
        DqtInductionStatus.NotYetCompleted => "Not yet completed",
        DqtInductionStatus.Pass => "Pass",
        DqtInductionStatus.PassedInWales => "Passed in Wales",
        DqtInductionStatus.RequiredToComplete => "Required to complete",
        _ => throw new ArgumentException($"Unknown {nameof(DqtInductionStatus)}: '{input}'.")
    };
}

using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public enum InductionStatus
{
    Exempt = 1,
    Fail = 2,
    FailedInWales = 3,
    InductionExtended = 4,
    InProgress = 5,
    NotYetCompleted = 6,
    Pass = 7,
    PassedInWales = 8,
    RequiredToComplete = 9,
}

public static class InductionStatusExtensions
{
    public static InductionStatus ConvertToInductionStatus(this dfeta_InductionStatus input) => input switch
    {
        dfeta_InductionStatus.Exempt => InductionStatus.Exempt,
        dfeta_InductionStatus.Fail => InductionStatus.Fail,
        dfeta_InductionStatus.FailedinWales => InductionStatus.FailedInWales,
        dfeta_InductionStatus.InductionExtended => InductionStatus.InductionExtended,
        dfeta_InductionStatus.InProgress => InductionStatus.InProgress,
        dfeta_InductionStatus.NotYetCompleted => InductionStatus.NotYetCompleted,
        dfeta_InductionStatus.Pass => InductionStatus.Pass,
        dfeta_InductionStatus.PassedinWales => InductionStatus.PassedInWales,
        dfeta_InductionStatus.RequiredtoComplete => InductionStatus.RequiredToComplete,
        _ => throw new ArgumentException($"Unknown {nameof(InductionStatus)}: '{input}'.")
    };

    public static string GetDescription(this dfeta_InductionStatus input) => ConvertToInductionStatus(input).GetDescription();

    public static string GetDescription(this InductionStatus input) => input switch
    {
        InductionStatus.Exempt => "Exempt",
        InductionStatus.Fail => "Fail",
        InductionStatus.FailedInWales => "Failed in Wales",
        InductionStatus.InductionExtended => "Extended",
        InductionStatus.InProgress => "In progress",
        InductionStatus.NotYetCompleted => "Not yet completed",
        InductionStatus.Pass => "Pass",
        InductionStatus.PassedInWales => "Passed in Wales",
        InductionStatus.RequiredToComplete => "Required to complete",
        _ => throw new ArgumentException($"Unknown {nameof(InductionStatus)}: '{input}'.")
    };
}

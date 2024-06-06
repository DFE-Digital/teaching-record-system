using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public enum InductionStatus
{
    Exempt = 1,
    Fail = 2,
    FailedinWales = 3,
    InductionExtended = 4,
    InProgress = 5,
    NotYetCompleted = 6,
    Pass = 7,
    PassedinWales = 8,
    RequiredtoComplete = 9,
}

public static class InductionStatusExtensions
{
    public static InductionStatus ConvertToInductionStatus(this dfeta_InductionStatus input) =>
        input.ConvertToEnumByName<dfeta_InductionStatus, InductionStatus>();

    public static string GetDescription(this dfeta_InductionStatus input) => input switch
    {
        dfeta_InductionStatus.Exempt => "Exempt",
        dfeta_InductionStatus.Fail => "Fail",
        dfeta_InductionStatus.FailedinWales => "Failed in Wales",
        dfeta_InductionStatus.InductionExtended => "Extended",
        dfeta_InductionStatus.InProgress => "In progress",
        dfeta_InductionStatus.NotYetCompleted => "Not yet completed",
        dfeta_InductionStatus.Pass => "Pass",
        dfeta_InductionStatus.PassedinWales => "Passed in Wales",
        dfeta_InductionStatus.RequiredtoComplete => "Required to complete",
        _ => throw new ArgumentException($"Unknown {nameof(InductionStatus)}: '{input}'.")
    };
}

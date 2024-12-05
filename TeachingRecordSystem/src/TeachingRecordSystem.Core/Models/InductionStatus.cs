using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum InductionStatus
{
    [InductionStatusInfo("None", requiresStartDate: false, requiresCompletedDate: false)]
    None = 0,
    [InductionStatusInfo("Required to complete", requiresStartDate: false, requiresCompletedDate: false)]
    RequiredToComplete = 1,
    [InductionStatusInfo("Exempt", requiresStartDate: false, requiresCompletedDate: false)]
    Exempt = 2,
    [InductionStatusInfo("In progress", requiresStartDate: true, requiresCompletedDate: false)]
    InProgress = 3,
    [InductionStatusInfo("Passed", requiresStartDate: true, requiresCompletedDate: true)]
    Passed = 4,
    [InductionStatusInfo("Failed", requiresStartDate: true, requiresCompletedDate: true)]
    Failed = 5,
    [InductionStatusInfo("Failed in Wales", requiresStartDate: true, requiresCompletedDate: true)]
    FailedInWales = 6,
}

public static class InductionStatusRegistry
{
    private static readonly IReadOnlyDictionary<InductionStatus, InductionStatusInfo> _info =
        Enum.GetValues<InductionStatus>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<InductionStatusInfo> All => _info.Values.ToArray();

    public static string GetName(this InductionStatus status) => _info[status].Name;

    public static string GetTitle(this InductionStatus status) => _info[status].Title;

    public static bool RequiresStartDate(this InductionStatus status) => _info[status].RequiresStartDate;

    public static bool RequiresCompletedDate(this InductionStatus status) => _info[status].RequiresCompletedDate;

    public static InductionStatus ToInductionStatus(this dfeta_InductionStatus? status) => status switch
    {
        null => InductionStatus.None,
        dfeta_InductionStatus.RequiredtoComplete => InductionStatus.RequiredToComplete,
        dfeta_InductionStatus.NotYetCompleted => InductionStatus.InProgress,
        dfeta_InductionStatus.InProgress => InductionStatus.InProgress,
        dfeta_InductionStatus.InductionExtended => InductionStatus.InProgress,
        dfeta_InductionStatus.Pass => InductionStatus.Passed,
        dfeta_InductionStatus.Fail => InductionStatus.Failed,
        dfeta_InductionStatus.Exempt => InductionStatus.Exempt,
        dfeta_InductionStatus.PassedinWales => InductionStatus.Exempt,
        dfeta_InductionStatus.FailedinWales => InductionStatus.FailedInWales,
        _ => throw new ArgumentException($"Failed mapping '{status}' to {nameof(InductionStatus)}.", nameof(status))
    };

    private static InductionStatusInfo GetInfo(InductionStatus status)
    {
        var attr = status.GetType()
               .GetMember(status.ToString())
               .Single()
               .GetCustomAttribute<InductionStatusInfoAttribute>() ??
           throw new Exception($"{nameof(InductionStatus)}.{status} is missing the {nameof(InductionStatusInfoAttribute)} attribute.");

        return new InductionStatusInfo(status, attr.Name, attr.RequiresStartDate, attr.RequiresCompletedDate);
    }
}

public sealed record InductionStatusInfo(InductionStatus Value, string Name, bool RequiresStartDate, bool RequiresCompletedDate)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class InductionStatusInfoAttribute(string name, bool requiresStartDate, bool requiresCompletedDate) : Attribute
{
    public string Name => name;
    public bool RequiresStartDate => requiresStartDate;
    public bool RequiresCompletedDate => requiresCompletedDate;
}

using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum InductionStatus
{
    [InductionStatusInfo("none", 6, requiresStartDate: false, requiresCompletedDate: false)]
    None = 0,
    [InductionStatusInfo("required to complete", 5, requiresStartDate: false, requiresCompletedDate: false)]
    RequiredToComplete = 1,
    [InductionStatusInfo("exempt", 2, requiresStartDate: false, requiresCompletedDate: false, requiresExemptionReasons: true)]
    Exempt = 2,
    [InductionStatusInfo("in progress", 3, requiresStartDate: true, requiresCompletedDate: false)]
    InProgress = 3,
    [InductionStatusInfo("passed", 1, requiresStartDate: true, requiresCompletedDate: true)]
    Passed = 4,
    [InductionStatusInfo("failed", 0, requiresStartDate: true, requiresCompletedDate: true)]
    Failed = 5,
    [InductionStatusInfo("failed in Wales", 4, requiresStartDate: true, requiresCompletedDate: true)]
    FailedInWales = 6,
}

public static class InductionStatusRegistry
{
    private static readonly IReadOnlyDictionary<InductionStatus, InductionStatusInfo> _info =
        Enum.GetValues<InductionStatus>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<InductionStatusInfo> All => _info.Values.ToArray();

    public static ICollection<InductionStatusInfo> ValidStatusChangesWhenManagedByCpd =>
        _info
            .Where(s => s.Key is InductionStatus.Exempt or InductionStatus.FailedInWales)
            .Select(s => s.Value)
            .ToArray();

    public static string GetName(this InductionStatus status) => _info[status].Name;

    public static string GetTitle(this InductionStatus status) => _info[status].Title;

    public static bool RequiresStartDate(this InductionStatus status) => _info[status].RequiresStartDate;

    public static bool RequiresCompletedDate(this InductionStatus status) => _info[status].RequiresCompletedDate;

    public static bool RequiresExemptionReasons(this InductionStatus status) => _info[status].RequiresExemptionReasons;

    public static bool IsHigherPriorityThan(this InductionStatus status, InductionStatus otherStatus) =>
        status.GetPriority() < otherStatus.GetPriority();

    public static InductionStatus ToInductionStatus(this dfeta_InductionStatus status) =>
        ToInductionStatus((dfeta_InductionStatus?)status);

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

    private static int GetPriority(this InductionStatus status) => _info[status].Priority;

    private static InductionStatusInfo GetInfo(InductionStatus status)
    {
        var attr = status.GetType()
               .GetMember(status.ToString())
               .Single()
               .GetCustomAttribute<InductionStatusInfoAttribute>() ??
           throw new Exception($"{nameof(InductionStatus)}.{status} is missing the {nameof(InductionStatusInfoAttribute)} attribute.");

        return new InductionStatusInfo(status, attr.Priority, attr.Name, attr.RequiresStartDate, attr.RequiresCompletedDate, attr.RequiresExemptionReasons);
    }
}

public sealed record InductionStatusInfo(
    InductionStatus Value,
    int Priority,
    string Name,
    bool RequiresStartDate,
    bool RequiresCompletedDate,
    bool RequiresExemptionReasons = false)
{
    public string Title => Name[..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class InductionStatusInfoAttribute(
    string name,
    int priority,
    bool requiresStartDate,
    bool requiresCompletedDate,
    bool requiresExemptionReasons = false) : Attribute
{
    public string Name => name;
    public int Priority => priority;
    public bool RequiresStartDate => requiresStartDate;
    public bool RequiresCompletedDate => requiresCompletedDate;
    public bool RequiresExemptionReasons => requiresExemptionReasons;
}

using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum InductionStatus
{
    [InductionStatusInfo("none", 6, requiresQts: false, requiresStartDate: false, requiresCompletedDate: false)]
    None = 0,
    [InductionStatusInfo("required to complete", 5, requiresQts: true, requiresStartDate: false, requiresCompletedDate: false)]
    RequiredToComplete = 1,
    [InductionStatusInfo("exempt", 2, requiresQts: false, requiresStartDate: false, requiresCompletedDate: false, requiresExemptionReasons: true)]
    Exempt = 2,
    [InductionStatusInfo("in progress", 3, requiresQts: true, requiresStartDate: true, requiresCompletedDate: false)]
    InProgress = 3,
    [InductionStatusInfo("passed", 1, requiresQts: true, requiresStartDate: true, requiresCompletedDate: true)]
    Passed = 4,
    [InductionStatusInfo("failed", 0, requiresQts: true, requiresStartDate: true, requiresCompletedDate: true)]
    Failed = 5,
    [InductionStatusInfo("failed in Wales", 4, requiresQts: true, requiresStartDate: true, requiresCompletedDate: true)]
    FailedInWales = 6,
}

public static class InductionStatusRegistry
{
    private static readonly IReadOnlyDictionary<InductionStatus, InductionStatusInfo> _info =
        Enum.GetValues<InductionStatus>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<InductionStatusInfo> All => _info.Values.ToArray();

    public static IReadOnlyCollection<InductionStatusInfo> ValidStatusChangesWhenManagedByCpd =>
        _info
            .Where(s => s.Key is InductionStatus.Exempt or InductionStatus.FailedInWales)
            .Select(s => s.Value)
            .ToArray();

    public static string GetName(this InductionStatus status) => _info[status].Name;

    public static string GetTitle(this InductionStatus status) => _info[status].Title;

    public static bool RequiresQts(this InductionStatus status) => _info[status].RequiresQts;

    public static bool RequiresStartDate(this InductionStatus status) => _info[status].RequiresStartDate;

    public static bool RequiresCompletedDate(this InductionStatus status) => _info[status].RequiresCompletedDate;

    public static bool RequiresExemptionReasons(this InductionStatus status) => _info[status].RequiresExemptionReasons;

    public static bool IsHigherPriorityThan(this InductionStatus status, InductionStatus otherStatus) =>
        status.GetPriority() < otherStatus.GetPriority();

    public static string? ToDqtInductionStatus(this InductionStatus status, out string? statusDescription)
    {
        switch (status)
        {
            case InductionStatus.RequiredToComplete:
                statusDescription = "Required to Complete";
                return "RequiredtoComplete";
            case InductionStatus.Exempt:
                statusDescription = "Exempt";
                return "Exempt";
            case InductionStatus.InProgress:
                statusDescription = "In Progress";
                return "InProgress";
            case InductionStatus.Passed:
                statusDescription = "Pass";
                return "Pass";
            case InductionStatus.Failed:
                statusDescription = "Fail";
                return "Fail";
            case InductionStatus.FailedInWales:
                statusDescription = "Failed in Wales";
                return "FailedinWales";
            case InductionStatus.None:
            default:
                statusDescription = null;
                return null;
        }
    }

    private static int GetPriority(this InductionStatus status) => _info[status].Priority;

    private static InductionStatusInfo GetInfo(InductionStatus status)
    {
        var attr = status.GetType()
               .GetMember(status.ToString())
               .Single()
               .GetCustomAttribute<InductionStatusInfoAttribute>() ??
           throw new Exception($"{nameof(InductionStatus)}.{status} is missing the {nameof(InductionStatusInfoAttribute)} attribute.");

        return new InductionStatusInfo(status, attr.Priority, attr.Name, attr.RequiresQts, attr.RequiresStartDate, attr.RequiresCompletedDate, attr.RequiresExemptionReasons);
    }
}

public sealed record InductionStatusInfo(
    InductionStatus Value,
    int Priority,
    string Name,
    bool RequiresQts,
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
    bool requiresQts,
    bool requiresStartDate,
    bool requiresCompletedDate,
    bool requiresExemptionReasons = false) : Attribute
{
    public string Name => name;
    public int Priority => priority;
    public bool RequiresQts => requiresQts;
    public bool RequiresStartDate => requiresStartDate;
    public bool RequiresCompletedDate => requiresCompletedDate;
    public bool RequiresExemptionReasons => requiresExemptionReasons;
}

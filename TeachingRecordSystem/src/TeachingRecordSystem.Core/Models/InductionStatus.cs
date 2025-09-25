using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum InductionStatus
{
    [InductionStatusDescription("none", 6, requiresQts: false, requiresStartDate: false, requiresCompletedDate: false)]
    None = 0,
    [InductionStatusDescription("required to complete", 5, requiresQts: true, requiresStartDate: false, requiresCompletedDate: false)]
    RequiredToComplete = 1,
    [InductionStatusDescription("exempt", 2, requiresQts: false, requiresStartDate: false, requiresCompletedDate: false, requiresExemptionReasons: true)]
    Exempt = 2,
    [InductionStatusDescription("in progress", 3, requiresQts: true, requiresStartDate: true, requiresCompletedDate: false)]
    InProgress = 3,
    [InductionStatusDescription("passed", 1, requiresQts: true, requiresStartDate: true, requiresCompletedDate: true)]
    Passed = 4,
    [InductionStatusDescription("failed", 0, requiresQts: true, requiresStartDate: true, requiresCompletedDate: true)]
    Failed = 5,
    [InductionStatusDescription("failed in Wales", 4, requiresQts: true, requiresStartDate: true, requiresCompletedDate: true)]
    FailedInWales = 6,
}

public static class InductionStatusRegistry
{
    private static readonly IReadOnlyDictionary<InductionStatus, InductionStatusDescription> _info =
        Enum.GetValues<InductionStatus>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<InductionStatusDescription> All { get; } = _info.Values.ToArray();

    public static IReadOnlyCollection<InductionStatusDescription> ValidStatusChangesWhenManagedByCpd =>
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

    private static InductionStatusDescription GetInfo(InductionStatus status)
    {
        var attr = status.GetType()
               .GetMember(status.ToString())
               .Single()
               .GetCustomAttribute<InductionStatusDescriptionAttribute>() ??
           throw new Exception($"{nameof(InductionStatus)}.{status} is missing the {nameof(InductionStatusDescriptionAttribute)} attribute.");

        return new InductionStatusDescription(status, attr.Priority, attr.Name, attr.RequiresQts, attr.RequiresStartDate, attr.RequiresCompletedDate, attr.RequiresExemptionReasons);
    }
}

public sealed record InductionStatusDescription(
    InductionStatus InductionStatus,
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
file sealed class InductionStatusDescriptionAttribute(
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

using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum MandatoryQualificationStatus
{
    [MandatoryQualificationStatusInfo(name: "in progress")]
    InProgress = 0,
    [MandatoryQualificationStatusInfo(name: "passed")]
    Passed = 1,
    [MandatoryQualificationStatusInfo(name: "deferred")]
    Deferred = 2,
    [MandatoryQualificationStatusInfo(name: "extended")]
    Extended = 3,
    [MandatoryQualificationStatusInfo(name: "failed")]
    Failed = 4,
    [MandatoryQualificationStatusInfo(name: "withdrawn")]
    Withdrawn = 5
}

public static class MandatoryQualificationStatusRegistry
{
    private static readonly IReadOnlyDictionary<MandatoryQualificationStatus, MandatoryQualificationStatusInfo> _info =
        Enum.GetValues<MandatoryQualificationStatus>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<MandatoryQualificationStatusInfo> All => _info.Values.ToArray();

    public static string GetName(this MandatoryQualificationStatus status) => _info[status].Name;

    public static string GetTitle(this MandatoryQualificationStatus status) => _info[status].Title;

    private static MandatoryQualificationStatusInfo GetInfo(MandatoryQualificationStatus status)
    {
        var attr = status.GetType()
            .GetMember(status.ToString())
            .Single()
            .GetCustomAttribute<MandatoryQualificationStatusInfoAttribute>() ??
            throw new Exception($"{nameof(MandatoryQualificationStatus)}.{status} is missing the {nameof(MandatoryQualificationStatusInfoAttribute)} attribute.");

        return new MandatoryQualificationStatusInfo(status, attr.Name);
    }
}

public sealed record MandatoryQualificationStatusInfo(MandatoryQualificationStatus Value, string Name)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class MandatoryQualificationStatusInfoAttribute(string name) : Attribute
{
    public string Name => name;
}

using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum MandatoryQualificationStatus
{
    [MandatoryQualificationStatusInfo(name: "in progress", dfeta_qualification_dfeta_MQ_Status.InProgress)]
    InProgress = 0,
    [MandatoryQualificationStatusInfo(name: "passed", dfeta_qualification_dfeta_MQ_Status.Passed)]
    Passed = 1,
    [MandatoryQualificationStatusInfo(name: "deferred", dfeta_qualification_dfeta_MQ_Status.Deferred)]
    Deferred = 2,
    [MandatoryQualificationStatusInfo(name: "extended", dfeta_qualification_dfeta_MQ_Status.Extended)]
    Extended = 3,
    [MandatoryQualificationStatusInfo(name: "failed", dfeta_qualification_dfeta_MQ_Status.Failed)]
    Failed = 4,
    [MandatoryQualificationStatusInfo(name: "withdrawn", dfeta_qualification_dfeta_MQ_Status.Withdrawn)]
    Withdrawn = 5,
}

public static class MandatoryQualificationStatusRegistry
{
    private static readonly IReadOnlyDictionary<MandatoryQualificationStatus, MandatoryQualificationStatusInfo> _info =
        Enum.GetValues<MandatoryQualificationStatus>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<MandatoryQualificationStatusInfo> All => _info.Values.ToArray();

    public static string GetName(this MandatoryQualificationStatus status) => _info[status].Name;

    public static string GetTitle(this MandatoryQualificationStatus status) => _info[status].Title;

    public static dfeta_qualification_dfeta_MQ_Status GetDqtStatus(this MandatoryQualificationStatus status) => _info[status].DqtStatus;

    public static MandatoryQualificationStatus ToMandatoryQualificationStatus(this dfeta_qualification_dfeta_MQ_Status status) =>
        _info.Values.Single(s => s.DqtStatus == status, $"Failed mapping '{status}' to {nameof(MandatoryQualificationStatus)}.").Value;

    private static MandatoryQualificationStatusInfo GetInfo(MandatoryQualificationStatus status)
    {
        var attr = status.GetType()
            .GetMember(status.ToString())
            .Single()
            .GetCustomAttribute<MandatoryQualificationStatusInfoAttribute>() ??
            throw new Exception($"{nameof(MandatoryQualificationStatus)}.{status} is missing the {nameof(MandatoryQualificationStatusInfoAttribute)} attribute.");

        return new MandatoryQualificationStatusInfo(status, attr.Name, attr.DqtStatus);
    }
}

public sealed record MandatoryQualificationStatusInfo(MandatoryQualificationStatus Value, string Name, dfeta_qualification_dfeta_MQ_Status DqtStatus)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class MandatoryQualificationStatusInfoAttribute(string name, dfeta_qualification_dfeta_MQ_Status dqtStatus) : Attribute
{
    public string Name => name;
    public dfeta_qualification_dfeta_MQ_Status DqtStatus => dqtStatus;
}

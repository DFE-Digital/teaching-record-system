using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum SupportTaskType
{
    [SupportTaskTypeDescription("connect GOV.UK One Login user to a teaching record", SupportTaskCategory.OneLogin)]
    ConnectOneLoginUser = 1,

    [SupportTaskTypeDescription("change name request", SupportTaskCategory.ChangeRequests)]
    ChangeNameRequest = 2,

    [SupportTaskTypeDescription("change date of birth request", SupportTaskCategory.ChangeRequests)]
    ChangeDateOfBirthRequest = 3,

    [SupportTaskTypeDescription("TRN request from API", SupportTaskCategory.TrnRequests)]
    ApiTrnRequest = 4,

    [SupportTaskTypeDescription("TRN request from NPQ", SupportTaskCategory.TrnRequests)]
    NpqTrnRequest = 5,

    [SupportTaskTypeDescription("manual checks needed", SupportTaskCategory.TrnRequests)]
    TrnRequestManualChecksNeeded = 6,

    [SupportTaskTypeDescription("teacher pensions potential duplicate", SupportTaskCategory.TeacherPensions)]
    TeacherPensionsPotentialDuplicate = 7
}

public static class SupportTaskTypeRegistry
{
    private static readonly IReadOnlyDictionary<SupportTaskType, SupportTaskTypeDescription> _info =
        Enum.GetValues<SupportTaskType>().ToDictionary(s => s, GetInfo);

    public static IReadOnlyCollection<SupportTaskTypeDescription> GetAll() =>
        _info.Values.OrderBy(s => s.Title).ToArray();

    public static SupportTaskCategory GetCategory(this SupportTaskType supportTaskType) =>
        _info[supportTaskType].SupportTaskCategory;

    public static string GetName(this SupportTaskType supportTaskType) => _info[supportTaskType].Name;

    public static string GetTitle(this SupportTaskType supportTaskType) => _info[supportTaskType].Title;

    private static SupportTaskTypeDescription GetInfo(SupportTaskType supportTaskType)
    {
        var attr = supportTaskType.GetType()
            .GetMember(supportTaskType.ToString())
            .Single()
            .GetCustomAttribute<SupportTaskTypeDescriptionAttribute>() ??
            throw new Exception($"{nameof(SupportTaskType)}.{supportTaskType} is missing the {nameof(SupportTaskTypeDescriptionAttribute)} attribute.");

        return new SupportTaskTypeDescription(supportTaskType, attr.Name, attr.SupportTaskCategory);
    }
}

public sealed record SupportTaskTypeDescription(SupportTaskType SupportTaskType, string Name, SupportTaskCategory SupportTaskCategory)
{
    public string Title => Name[..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class SupportTaskTypeDescriptionAttribute(string name, SupportTaskCategory category) : Attribute
{
    public string Name => name;
    public SupportTaskCategory SupportTaskCategory => category;
}

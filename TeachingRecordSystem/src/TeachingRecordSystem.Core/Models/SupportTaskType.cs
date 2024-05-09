using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum SupportTaskType
{
    [SupportTaskTypeInfo("connect GOV.UK One Login user to a teaching record", SupportTaskCategory.OneLogin)]
    ConnectOneLoginUser = 1,
    [SupportTaskTypeInfo("change name request", SupportTaskCategory.ChangeRequests)]
    ChangeNameRequest = 2,
    [SupportTaskTypeInfo("change date of birth request", SupportTaskCategory.ChangeRequests)]
    ChangeDateOfBirthRequest = 3,
}

public static class SupportTaskTypeRegistry
{
    private static readonly IReadOnlyDictionary<SupportTaskType, SupportTaskTypeInfo> _info =
        Enum.GetValues<SupportTaskType>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<SupportTaskTypeInfo> GetAll() =>
        _info.Values.OrderBy(s => s.Title).ToArray();

    public static SupportTaskCategory GetCategory(this SupportTaskType supportTaskType) =>
        _info[supportTaskType].SupportTaskCategory;

    public static string GetName(this SupportTaskType supportTaskType) => _info[supportTaskType].Name;

    public static string GetTitle(this SupportTaskType supportTaskType) => _info[supportTaskType].Title;

    private static SupportTaskTypeInfo GetInfo(SupportTaskType supportTaskType)
    {
        var attr = supportTaskType.GetType()
            .GetMember(supportTaskType.ToString())
            .Single()
            .GetCustomAttribute<SupportTaskTypeInfoAttribute>() ??
            throw new Exception($"{nameof(SupportTaskType)}.{supportTaskType} is missing the {nameof(SupportTaskTypeInfoAttribute)} attribute.");

        return new SupportTaskTypeInfo(supportTaskType, attr.Name, attr.SupportTaskCategory);
    }
}

public sealed record SupportTaskTypeInfo(SupportTaskType Value, string Name, SupportTaskCategory SupportTaskCategory)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class SupportTaskTypeInfoAttribute(string name, SupportTaskCategory category) : Attribute
{
    public string Name => name;
    public SupportTaskCategory SupportTaskCategory => category;
}

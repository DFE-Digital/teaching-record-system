using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum SupportTaskCategory
{
    [SupportTaskCategoryInfo("GOV.UK One Login")]
    OneLogin = 1,
    [SupportTaskCategoryInfo("change requests")]
    ChangeRequests = 2,
}

public static class SupportTaskCategoryRegistry
{
    private static readonly IReadOnlyDictionary<SupportTaskCategory, SupportTaskCategoryInfo> _info =
        Enum.GetValues<SupportTaskCategory>().ToDictionary(s => s, s => GetInfo(s));

    public static IReadOnlyCollection<SupportTaskCategoryInfo> GetAll() =>
        _info.Values.OrderBy(s => s.Title).ToArray();

    public static string GetName(this SupportTaskCategory supportTaskCategory) => _info[supportTaskCategory].Name;

    public static string GetTitle(this SupportTaskCategory supportTaskCategory) => _info[supportTaskCategory].Title;

    public static SupportTaskCategory GetSupportTaskCategoryForType(SupportTaskType supportTaskType) =>
        SupportTaskTypeRegistry.GetCategory(supportTaskType);

    public static IReadOnlyCollection<SupportTaskType> GetSupportTaskTypesByCategory(SupportTaskCategory supportTaskCategory) =>
        SupportTaskTypeRegistry.GetAll().Where(v => v.SupportTaskCategory == supportTaskCategory).Select(i => i.Value).AsReadOnly();

    private static SupportTaskCategoryInfo GetInfo(SupportTaskCategory supportTaskCategory)
    {
        var attr = supportTaskCategory.GetType()
            .GetMember(supportTaskCategory.ToString())
            .Single()
            .GetCustomAttribute<SupportTaskCategoryInfoAttribute>() ??
            throw new Exception($"{nameof(SupportTaskCategory)}.{supportTaskCategory} is missing the {nameof(SupportTaskCategoryInfoAttribute)} attribute.");

        return new SupportTaskCategoryInfo(supportTaskCategory, attr.Name);
    }
}

public sealed record SupportTaskCategoryInfo(SupportTaskCategory Value, string Name)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class SupportTaskCategoryInfoAttribute(string name) : Attribute
{
    public string Name => name;
}

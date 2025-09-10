using System.Reflection;

namespace TeachingRecordSystem.Core.Models;

public enum SupportTaskType
{
    [SupportTaskTypeInfo("connect GOV.UK One Login user to a teaching record", SupportTaskCategory.OneLogin, "4b76f9b8-c60e-4076-ac4b-d173f395dc71")]
    ConnectOneLoginUser = 1,
    [SupportTaskTypeInfo("change name request", SupportTaskCategory.ChangeRequests, "6bc82e72-7592-4b05-a4ae-822fb52cad8d")]
    ChangeNameRequest = 2,
    [SupportTaskTypeInfo("change date of birth request", SupportTaskCategory.ChangeRequests, "b621cc79-b116-461e-be8d-593d6efd53cd")]
    ChangeDateOfBirthRequest = 3,
    [SupportTaskTypeInfo("TRN request from API", SupportTaskCategory.TrnRequests, "37c27275-829c-4aa0-a47c-62a0092d0a71")]
    ApiTrnRequest = 4,
    [SupportTaskTypeInfo("TRN request from NPQ", SupportTaskCategory.TrnRequests, "3ca684d4-15de-4f12-b0fb-c5386360b171")]
    NpqTrnRequest = 5,
    [SupportTaskTypeInfo("Manual checks needed", SupportTaskCategory.TrnRequests, "80adb2e0-199c-4629-b494-4d052230a248")]
    TrnRequestManualChecksNeeded = 6,
    [SupportTaskTypeInfo("Capita import potential duplicate", SupportTaskCategory.CapitaImport, "fdee6a10-6338-463a-b6df-e34a2b95a854")]
    CapitaImportPotentialDuplicate = 7
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

    public static Guid GetSupportTaskTypeId(this SupportTaskType supportTaskType) => _info[supportTaskType].SupportTaskTypeId;

    private static SupportTaskTypeInfo GetInfo(SupportTaskType supportTaskType)
    {
        var attr = supportTaskType.GetType()
            .GetMember(supportTaskType.ToString())
            .Single()
            .GetCustomAttribute<SupportTaskTypeInfoAttribute>() ??
            throw new Exception($"{nameof(SupportTaskType)}.{supportTaskType} is missing the {nameof(SupportTaskTypeInfoAttribute)} attribute.");

        return new SupportTaskTypeInfo(supportTaskType, attr.Name, attr.SupportTaskCategory, attr.SupportTaskTypeId);
    }
}

public sealed record SupportTaskTypeInfo(SupportTaskType Value, string Name, SupportTaskCategory SupportTaskCategory, Guid SupportTaskTypeId)
{
    public string Title => Name[0..1].ToUpper() + Name[1..];
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
file sealed class SupportTaskTypeInfoAttribute(string name, SupportTaskCategory category, string supportTaskTypeId) : Attribute
{
    public Guid SupportTaskTypeId { get; } = new Guid(supportTaskTypeId);
    public string Name => name;
    public SupportTaskCategory SupportTaskCategory => category;
}

using System.Reflection;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTaskType
{
    private SupportTaskType(Guid supportTaskTypeId, string name, SupportTaskCategory category)
    {
        SupportTaskTypeId = supportTaskTypeId;
        Name = name;
        Category = category;
        DataType = GetDataTypeForSupportTaskTypeId(supportTaskTypeId);
    }

    public static IReadOnlyCollection<SupportTaskType> All =>
    [
        ConnectOneLoginUser,
        ChangeNameRequest,
        ChangeDateOfBirthRequest,
        ApiTrnRequest,
        NpqTrnRequest,
        TrnRequestManualChecksNeeded,
        CapitaImportPotentialDuplicate
    ];

    public static SupportTaskType ConnectOneLoginUser { get; } =
        new(new("4b76f9b8-c60e-4076-ac4b-d173f395dc71"), "connect GOV.UK One Login user to a teaching record", SupportTaskCategory.OneLogin);

    public static SupportTaskType ChangeNameRequest { get; } =
        new(new("6bc82e72-7592-4b05-a4ae-822fb52cad8d"), "change name request", SupportTaskCategory.ChangeRequests);

    public static SupportTaskType ChangeDateOfBirthRequest { get; } =
        new(new("b621cc79-b116-461e-be8d-593d6efd53cd"), "change date of birth request", SupportTaskCategory.ChangeRequests);

    public static SupportTaskType ApiTrnRequest { get; } =
        new(new("37c27275-829c-4aa0-a47c-62a0092d0a71"), "TRN request from API", SupportTaskCategory.TrnRequests);

    public static SupportTaskType NpqTrnRequest { get; } =
        new(new("3ca684d4-15de-4f12-b0fb-c5386360b171"), "TRN request from NPQ", SupportTaskCategory.TrnRequests);

    public static SupportTaskType TrnRequestManualChecksNeeded { get; } =
        new(new("80adb2e0-199c-4629-b494-4d052230a248"), "manual checks needed", SupportTaskCategory.TrnRequests);

    public static SupportTaskType CapitaImportPotentialDuplicate { get; } =
        new(new("fdee6a10-6338-463a-b6df-e34a2b95a854"), "Capita import potential duplicate", SupportTaskCategory.CapitaImport);

    public Guid SupportTaskTypeId { get; }
    public string Name { get; }
    public SupportTaskCategory Category { get; }
    public string Title => Name[..1].ToUpper() + Name[1..];
    public Type DataType { get; }

    internal static SupportTaskType FromSupportTaskTypeId(Guid supportTaskTypeId) =>
        All.Single(s => s.SupportTaskTypeId == supportTaskTypeId);

    internal static Type GetDataTypeForSupportTaskTypeId(Guid supportTaskTypeId) =>
        typeof(SupportTaskType).Assembly.GetTypes()
            .SingleOrDefault(t => t.GetCustomAttribute<SupportTaskDataAttribute>()?.SupportTaskTypeId == supportTaskTypeId) ??
        throw new InvalidOperationException($"Cannot find support task data type for support task type ID '{supportTaskTypeId}'");

    public override string ToString() => Title;
}

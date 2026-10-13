namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public sealed record ChangeHistoryContext
{
    private readonly ChangeHistoryService.ContextData _contextData;

    private ChangeHistoryContext(ChangeHistoryService.ContextData contextData)
    {
        _contextData = contextData;
    }

    public static ChangeHistoryContext ForPerson(Guid personId, ChangeHistoryService.ContextData contextData) =>
        new(contextData)
        {
            ContextType = ChangeHistoryContextType.Person,
            PersonId = personId
        };

    public static ChangeHistoryContext ForSupportTask(string supportTaskReference, ChangeHistoryService.ContextData contextData) =>
        new(contextData)
        {
            ContextType = ChangeHistoryContextType.SupportTask,
            SupportTaskReference = supportTaskReference
        };

    public static ChangeHistoryContext ForOneLoginUser(string oneLoginUserSubject, ChangeHistoryService.ContextData contextData) =>
        new(contextData)
        {
            ContextType = ChangeHistoryContextType.OneLogin,
            OneLoginUserSubject = oneLoginUserSubject
        };

    public ChangeHistoryContextType ContextType { get; private set; }

    public Guid PersonId
    {
        get => ContextType is ChangeHistoryContextType.Person
            ? field
            : throw new InvalidOperationException($"{nameof(ContextType)} does not have a {nameof(PersonId)}.");
        private set;
    }

    public string SupportTaskReference
    {
        get => ContextType is ChangeHistoryContextType.SupportTask
            ? field!
            : throw new InvalidOperationException(
                $"{nameof(ContextType)} does not have a {nameof(SupportTaskReference)}.");
        private set;
    }

    public string OneLoginUserSubject
    {
        get => ContextType is ChangeHistoryContextType.OneLogin
            ? field!
            : throw new InvalidOperationException(
                $"{nameof(ContextType)} does not have a {nameof(OneLoginUserSubject)}.");
        private set;
    }

    public PersonInfo GetPersonInfo(Guid personId) => _contextData.AllPersons[personId];

    public PersonInfo? TryGetPersonInfo(Guid personId) => _contextData.AllPersons.TryGetValue(personId, out var personInfo) ? personInfo : null;

    public OneLoginUserInfo GetOneLoginUserInfo(string subject) => _contextData.AllOneLoginUsers[subject];

    public string? TryGetUserName(Guid userId) => _contextData.AllUserNames.TryGetValue(userId, out var name) ? name : null;

    public ApplicationUserInfo? TryGetApplicationUserInfo(Guid userId) =>
        _contextData.AllApplicationUsers.TryGetValue(userId, out var applicationUserInfo) ? applicationUserInfo : null;

    public record PersonInfo(Guid PersonId, string Trn, string FirstName, string LastName);

    public record OneLoginUserInfo(string OneLoginUserSubject, string? EmailAddress);

    public record ApplicationUserInfo(Guid UserId, string Name, string? ShortName);
}

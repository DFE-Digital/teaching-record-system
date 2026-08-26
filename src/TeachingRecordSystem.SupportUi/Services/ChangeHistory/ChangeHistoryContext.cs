namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public sealed record ChangeHistoryContext
{
    private readonly IReadOnlyDictionary<Guid, PersonInfo> _allPersons;
    private readonly IReadOnlyDictionary<string, OneLoginUserInfo> _allOneLoginUsers;

    private ChangeHistoryContext(
        IReadOnlyDictionary<Guid, PersonInfo> allPersons,
        IReadOnlyDictionary<string, OneLoginUserInfo> allOneLoginUsers)
    {
        _allPersons = allPersons;
        _allOneLoginUsers = allOneLoginUsers;
    }

    public static ChangeHistoryContext ForPerson(
        Guid personId,
        IReadOnlyDictionary<Guid, PersonInfo> allPersons,
        IReadOnlyDictionary<string, OneLoginUserInfo> allOneLoginUsers) =>
        new(allPersons, allOneLoginUsers)
        {
            ContextType = ChangeHistoryContextType.Person,
            PersonId = personId
        };

    public static ChangeHistoryContext ForSupportTask(
        string supportTaskReference,
        IReadOnlyDictionary<Guid, PersonInfo> allPersons,
        IReadOnlyDictionary<string, OneLoginUserInfo> allOneLoginUsers) =>
        new(allPersons, allOneLoginUsers)
        {
            ContextType = ChangeHistoryContextType.SupportTask,
            SupportTaskReference = supportTaskReference
        };

    public static ChangeHistoryContext ForOneLoginUser(
        string oneLoginUserSubject,
        IReadOnlyDictionary<Guid, PersonInfo> allPersons,
        IReadOnlyDictionary<string, OneLoginUserInfo> allOneLoginUsers) =>
        new(allPersons, allOneLoginUsers)
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

    public PersonInfo GetPersonInfo(Guid personId) => _allPersons[personId];

    public OneLoginUserInfo GetOneLoginUserInfo(string subject) => _allOneLoginUsers[subject];

    public record PersonInfo(Guid PersonId, string Trn, string FirstName, string LastName);

    public record OneLoginUserInfo(string OneLoginUserSubject, string? EmailAddress);
}

namespace TeachingRecordSystem.SupportUi.Pages.Shared.Components.ChangeHistoryEntry;

public sealed record ChangeHistoryContext
{
    private ChangeHistoryContext() { }

    public static ChangeHistoryContext ForPerson(Guid personId) => new()
    {
        ContextType = ChangeHistoryContextType.Person,
        PersonId = personId
    };

    public ChangeHistoryContextType ContextType { get; private set; }

    public Guid PersonId
    {
        get => ContextType is ChangeHistoryContextType.Person
            ? field
            : throw new InvalidOperationException($"{nameof(ContextType)} does not have a {nameof(PersonId)}.");
        private set;
    }
}

namespace TeachingRecordSystem.Core.Models;

public enum ProcessType
{
    PersonMigratingFromDqt = 1,
    PersonCreatingInDqt = 2,
    PersonImportingIntoDqt = 3,
    PersonUpdatingInDqt = 4,
    PersonDeactivatingInDqt = 5,
    PersonReactivatingInDqt = 6,
    PersonMergingInDqt = 7,
    SupportTaskDeleting = 8,
    TrnRequestResetting = 9,
    NotifyingTrnRecipient = 10,
    NoteCreating = 11,
    NoteCreatingInDqt = 12,
    NoteImportingIntoDqt = 13,
    NoteUpdatingInDqt = 14
}

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
    ChangeOfDateOfBirthRequestCreating = 12,
    ChangeOfNameRequestCreating = 13,
    ApiTrnRequestCreating = 14,
    NpqTrnRequestTaskCreating = 15,
    ApiTrnRequestResolving = 16,
    ConnectOneLoginUserSupportTaskCreating = 17,
    NpqTrnRequestApproving = 18,
    NpqTrnRequestRejecting = 19
}

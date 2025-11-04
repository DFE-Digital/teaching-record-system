namespace TeachingRecordSystem.Core.Events;

public record PersonUpdatedInDqtEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required PersonUpdatedInDqtEventChanges Changes { get; init; }
    public required EventModels.DqtPersonDetails Details { get; init; }
    public required EventModels.DqtPersonDetails OldDetails { get; init; }
}

[Flags]
public enum PersonUpdatedInDqtEventChanges
{
    None = 0,
    Trn = 1 << 0,
    FirstName = 1 << 1,
    MiddleName = 1 << 2,
    LastName = 1 << 3,
    DateOfBirth = 1 << 4,
    EmailAddress = 1 << 5,
    NationalInsuranceNumber = 1 << 6,
    Gender = 1 << 7,
    DateOfDeath = 1 << 8,
    QtsDate = 1 << 9,
    EytsDate = 1 << 10,
    QtlsDate = 1 << 11,
    QtlsStatus = 1 << 12,
    InductionStatus = 1 << 13,
    DqtInductionStatus = 1 << 14,

    All = Trn |
        FirstName |
        MiddleName |
        LastName |
        DateOfBirth |
        EmailAddress |
        NationalInsuranceNumber |
        Gender |
        DateOfDeath |
        QtsDate |
        EytsDate |
        QtlsDate |
        QtlsStatus |
        InductionStatus |
        DqtInductionStatus
}

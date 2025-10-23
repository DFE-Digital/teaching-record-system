namespace TeachingRecordSystem.Core.Events;

public record PersonUpdatedInDqtEvent : IEvent
{
    public required Guid EventId { get; init; }
    public Guid[] PersonIds => [PersonId];
    public required Guid PersonId { get; init; }
    public required PersonUpdatedInDqtEventChanges Changes { get; init; }
    public required string? Trn { get; init; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }
    public required string? EmailAddress { get; set; }
    public required string? NationalInsuranceNumber { get; set; }
    public required Gender? Gender { get; set; }
    public required DateOnly? DateOfDeath { get; init; }
    public required DateOnly? QtsDate { get; init; }
    public required DateOnly? EytsDate { get; init; }
    public required DateOnly? QtlsDate { get; init; }
    public required QtlsStatus QtlsStatus { get; init; }
    public required InductionStatus? InductionStatus { get; init; }
    public required string? DqtInductionStatus { get; init; }
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

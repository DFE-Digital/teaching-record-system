namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateMandatoryQualificationSpecialismQuery(
    Guid QualificationId,
    Guid SpecialismId,
    EventBase Event) : ICrmQuery<bool>;

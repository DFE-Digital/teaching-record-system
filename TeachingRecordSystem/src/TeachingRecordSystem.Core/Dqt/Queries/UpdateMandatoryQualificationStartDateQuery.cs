namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateMandatoryQualificationStartDateQuery(
    Guid QualificationId,
    DateOnly StartDate,
    EventBase Event) : ICrmQuery<bool>;

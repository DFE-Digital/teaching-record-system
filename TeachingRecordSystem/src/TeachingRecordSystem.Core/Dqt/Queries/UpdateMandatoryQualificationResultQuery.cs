namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateMandatoryQualificationStatusQuery(
    Guid QualificationId,
    dfeta_qualification_dfeta_MQ_Status MqStatus,
    DateOnly? EndDate,
    EventBase Event) : ICrmQuery<bool>;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record DeleteQualificationQuery(Guid QualificationId, EventBase Event) : ICrmQuery<bool>;

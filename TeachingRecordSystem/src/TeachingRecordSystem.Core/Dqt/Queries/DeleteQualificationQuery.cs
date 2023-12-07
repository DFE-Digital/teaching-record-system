namespace TeachingRecordSystem.Core.Dqt.Queries;

public record DeleteQualificationQuery(Guid QualificationId, string SerializedEvent) : ICrmQuery<bool>;

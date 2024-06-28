namespace TeachingRecordSystem.Core.Dqt.Queries;

public record DeleteAnnotationQuery(Guid AnnotationId, EventInfo Event) : ICrmQuery<bool>;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateDqtOutboxMessageQuery(object Message) : ICrmQuery<Guid>;

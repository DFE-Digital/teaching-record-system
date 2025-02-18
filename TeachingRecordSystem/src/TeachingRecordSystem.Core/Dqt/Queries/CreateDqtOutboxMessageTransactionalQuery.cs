namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CreateDqtOutboxMessageTransactionalQuery(object Message) : ICrmTransactionalQuery<Guid>;

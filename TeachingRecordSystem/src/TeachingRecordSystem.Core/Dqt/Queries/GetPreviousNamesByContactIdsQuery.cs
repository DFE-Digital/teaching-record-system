namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetPreviousNamesByContactIdsQuery(IEnumerable<Guid> ContactIds) : ICrmQuery<IDictionary<Guid, dfeta_previousname[]>>;

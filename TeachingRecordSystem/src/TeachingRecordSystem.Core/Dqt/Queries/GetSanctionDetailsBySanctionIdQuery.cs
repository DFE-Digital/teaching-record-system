namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetSanctionDetailsBySanctionIdQuery(Guid SanctionId) : ICrmQuery<SanctionDetailResult?>;

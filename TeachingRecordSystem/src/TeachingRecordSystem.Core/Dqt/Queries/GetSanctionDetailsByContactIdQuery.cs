namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetSanctionDetailsByContactIdQuery(Guid ContactId) : ICrmQuery<SanctionDetailResult[]>;

public record SanctionDetailResult(dfeta_sanction Sanction, string Description);

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateSanctionStateQuery(Guid SanctionId, dfeta_sanctionState State) : ICrmQuery<bool>;

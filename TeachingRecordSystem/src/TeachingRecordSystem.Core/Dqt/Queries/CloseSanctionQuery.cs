namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CloseSanctionQuery(Guid SanctionId, DateOnly EndDate) : ICrmQuery<bool>;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record SetDeceasedQuery(Guid ContactId, DateOnly DateOfDeath) : ICrmQuery<bool>;

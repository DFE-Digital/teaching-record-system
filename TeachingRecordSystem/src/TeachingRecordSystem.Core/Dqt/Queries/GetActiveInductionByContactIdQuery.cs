namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveInductionByContactIdQuery(Guid ContactId) : ICrmQuery<InductionRecord>;

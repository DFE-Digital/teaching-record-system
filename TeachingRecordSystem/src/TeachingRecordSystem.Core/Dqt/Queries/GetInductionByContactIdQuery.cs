namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetInductionByContactIdQuery(Guid ContactId) : ICrmQuery<InductionRecord>;

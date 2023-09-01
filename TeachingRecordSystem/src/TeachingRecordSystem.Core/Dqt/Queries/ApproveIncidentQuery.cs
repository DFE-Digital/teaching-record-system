namespace TeachingRecordSystem.Core.Dqt.Queries;

public record ApproveIncidentQuery(Guid IncidentId) : ICrmQuery<bool>;

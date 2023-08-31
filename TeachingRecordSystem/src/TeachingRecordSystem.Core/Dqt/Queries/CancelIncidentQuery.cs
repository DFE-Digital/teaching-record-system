namespace TeachingRecordSystem.Core.Dqt.Queries;

public record CancelIncidentQuery(Guid IncidentId) : ICrmQuery<bool>;

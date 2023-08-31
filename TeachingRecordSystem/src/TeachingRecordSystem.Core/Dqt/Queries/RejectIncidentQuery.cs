namespace TeachingRecordSystem.Core.Dqt.Queries;

public record RejectIncidentQuery(Guid IncidentId, string RejectionReason) : ICrmQuery<bool>;

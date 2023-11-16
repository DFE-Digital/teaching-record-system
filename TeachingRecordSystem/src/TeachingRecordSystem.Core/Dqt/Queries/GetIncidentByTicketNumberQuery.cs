namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetIncidentByTicketNumberQuery(string TicketNumber) : ICrmQuery<IncidentDetail?>;

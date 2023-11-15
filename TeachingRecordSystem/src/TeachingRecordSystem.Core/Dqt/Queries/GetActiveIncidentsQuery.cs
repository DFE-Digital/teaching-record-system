namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveIncidentsQuery(int PageNumber, int PageSize) : ICrmQuery<GetIncidentsResult>;

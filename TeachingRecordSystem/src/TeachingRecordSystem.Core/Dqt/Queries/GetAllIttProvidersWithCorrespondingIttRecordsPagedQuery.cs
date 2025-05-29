namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(int PageNumber, int Pagesize, string? PagingCookie = null) : ICrmQuery<PagedProviderResults>;

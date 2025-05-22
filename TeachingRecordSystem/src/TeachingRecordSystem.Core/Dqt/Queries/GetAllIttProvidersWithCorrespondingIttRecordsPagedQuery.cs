namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(int pageNumber, int pagesize, string? pagingCookie = null) : ICrmQuery<PagedProviderResults>;

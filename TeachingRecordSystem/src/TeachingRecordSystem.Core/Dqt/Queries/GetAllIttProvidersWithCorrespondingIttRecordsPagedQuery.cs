namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetAllIttProvidersWithCorrespondingIttRecordsPagedQuery(int pageNumber, string? pagingCookie = null) : ICrmQuery<PagedProviderResults>;

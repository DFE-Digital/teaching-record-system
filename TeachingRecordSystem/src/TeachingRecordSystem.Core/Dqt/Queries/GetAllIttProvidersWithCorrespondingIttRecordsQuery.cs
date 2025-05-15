namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetAllIttProvidersWithCorrespondingIttRecordsQuery(int pageNumber, string? pagingCookie = null) : ICrmQuery<PagedProviderResults>;

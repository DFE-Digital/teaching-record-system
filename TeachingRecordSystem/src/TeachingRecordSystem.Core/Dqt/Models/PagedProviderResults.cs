namespace TeachingRecordSystem.Core.Dqt.Models;

public record PagedProviderResults(Account[] Providers, bool MoreRecords, string? PagingCookie);


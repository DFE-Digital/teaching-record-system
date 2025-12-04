using System.Globalization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public class SupportTaskSearchService(TrsDbContext dbContext)
{
    public async Task<ApiTrnRequestsSearchResult> SearchApiTrnRequestsAsync(ApiTrnRequestsSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? ApiTrnRequestsSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest && t.Status == SupportTaskStatus.Open);

        var totalTaskCount = await tasks.CountAsync();

        if (SearchTextIsDate(search, out var minDate, out var maxDate))
        {
            tasks = tasks.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextIsEmailAddress(search, out var email))
        {
            tasks = tasks.Where(t =>
                t.TrnRequestMetadata!.EmailAddress != null &&
                EF.Functions.Collate(t.TrnRequestMetadata.EmailAddress, Collations.CaseInsensitive) == email);
        }
        else if (SearchTextIsName(search, out var nameParts))
        {
            tasks = tasks.Where(t =>
                nameParts.All(n => t.TrnRequestMetadata!.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
        }

        var totalFilteredTaskCount = await tasks.CountAsync();

        tasks = sortBy switch
        {
            ApiTrnRequestsSortByOption.Name => tasks
                .OrderBy(t => t.TrnRequestMetadata!.FirstName, sortDirection)
                .ThenBy(t => t.TrnRequestMetadata!.MiddleName, sortDirection)
                .ThenBy(t => t.TrnRequestMetadata!.LastName, sortDirection),
            ApiTrnRequestsSortByOption.Email => tasks
                .OrderBy(t => t.TrnRequestMetadata!.EmailAddress, sortDirection),
            ApiTrnRequestsSortByOption.Source => tasks
                .OrderBy(t => t.TrnRequestMetadata!.ApplicationUser!.Name, sortDirection),
            _ => tasks
                .OrderBy(t => t.CreatedOn, sortDirection)
        };

        var searchResuts = await tasks
            .Select(t => new ApiTrnRequestsSearchResultItem(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.MiddleName ?? "",
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.EmailAddress,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser!.Name))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.ItemsPerPage, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = totalTaskCount,
            SearchResults = searchResuts
        };
    }

    public async Task<NpqTrnRequestsSearchResult> SearchNpqTrnRequestsAsync(NpqTrnRequestsSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? NpqTrnRequestsSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest && t.Status == SupportTaskStatus.Open);

        var totalTaskCount = await tasks.CountAsync();

        if (SearchTextIsDate(search, out var minDate, out var maxDate))
        {
            tasks = tasks.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextIsEmailAddress(search, out var email))
        {
            tasks = tasks.Where(t =>
                t.TrnRequestMetadata!.EmailAddress != null &&
                EF.Functions.Collate(t.TrnRequestMetadata.EmailAddress, Collations.CaseInsensitive) == email);
        }
        else if (SearchTextIsName(search, out var nameParts))
        {
            tasks = tasks.Where(t =>
                nameParts.All(n => t.TrnRequestMetadata!.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
        }

        var totalFilteredTaskCount = await tasks.CountAsync();

        tasks = sortBy switch
        {
            NpqTrnRequestsSortByOption.Name => tasks
                .OrderBy(t => t.TrnRequestMetadata!.FirstName, sortDirection)
                .ThenBy(t => t.TrnRequestMetadata!.MiddleName, sortDirection)
                .ThenBy(t => t.TrnRequestMetadata!.LastName, sortDirection),
            NpqTrnRequestsSortByOption.Email => tasks
                .OrderBy(t => t.TrnRequestMetadata!.EmailAddress, sortDirection),
            NpqTrnRequestsSortByOption.PotentialDuplicate => tasks
                .OrderBy(t => t.TrnRequestMetadata!.PotentialDuplicate, sortDirection),
            _ => tasks
                .OrderBy(t => t.CreatedOn, sortDirection)
        };

        var searchResults = await tasks
            .Select(t => new NpqTrnRequestsSearchResultItem(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.MiddleName ?? "",
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.EmailAddress,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser!.Name,
                t.TrnRequestMetadata.PotentialDuplicate))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.ItemsPerPage, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = totalTaskCount,
            SearchResults = searchResults
        };
    }

    public async Task<ChangeRequestsSearchResult> SearchChangeRequestsAsync(ChangeRequestsSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? ChangeRequestsSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;
        var changeRequestTypes = searchOptions.ChangeRequestTypes ?? [SupportTaskType.ChangeNameRequest, SupportTaskType.ChangeDateOfBirthRequest];

        var tasks = dbContext.SupportTasks
            .Include(t => t.Person)
            .Where(t => (t.SupportTaskType == SupportTaskType.ChangeNameRequest || t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest)
                        && t.Status == SupportTaskStatus.Open);

        var nameChangeRequestCount = await tasks.CountAsync(t => t.SupportTaskType == SupportTaskType.ChangeNameRequest);
        var dateOfBirthChangeRequestCount = await tasks.CountAsync(t => t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest);

        tasks = tasks.Where(t => changeRequestTypes.Contains(t.SupportTaskType));

        if (SearchTextIsName(search, out var nameParts))
        {
            tasks = tasks.Where(t =>
                nameParts.All(n => EF.Property<string[]>(t.Person!, "names").Contains(n)));
        }

        var totalFilteredTaskCount = await tasks.CountAsync();

        tasks = sortBy switch
        {
            ChangeRequestsSortByOption.Name => tasks
                .OrderBy(t => t.Person!.FirstName, sortDirection)
                .ThenBy(t => t.Person!.MiddleName, sortDirection)
                .ThenBy(t => t.Person!.LastName, sortDirection),
            ChangeRequestsSortByOption.ChangeType => tasks
                .OrderBy(t => t.SupportTaskType, sortDirection),
            _ => tasks
                .OrderBy(t => t.CreatedOn, sortDirection)
        };

        var searchResults = await tasks
            .Select(t => new ChangeRequestsSearchResultItem(
                t.SupportTaskReference,
                t.Person!.FirstName,
                t.Person.MiddleName,
                t.Person.LastName,
                StringHelper.JoinNonEmpty(' ', t.Person.FirstName, t.Person.MiddleName, t.Person.LastName),
                t.CreatedOn,
                t.SupportTaskType))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.ItemsPerPage, totalFilteredTaskCount);

        return new()
        {
            NameChangeRequestCount = nameChangeRequestCount,
            DateOfBirthChangeRequestCount = dateOfBirthChangeRequestCount,
            TotalRequestCount = nameChangeRequestCount + dateOfBirthChangeRequestCount,
            SearchResults = searchResults
        };
    }

    public async Task<TrnRequestManualChecksSearchResult> SearchTrnRequestManualChecksAsync(TrnRequestManualChecksSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? TrnRequestManualChecksSortByOption.DateCreated;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var baseQuery = dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded && t.Status == SupportTaskStatus.Open);

        var groupedBySource = await baseQuery
            .Join(dbContext.ApplicationUsers, t => t.TrnRequestMetadata!.ApplicationUserId, u => u.UserId, (t, u) => new { Task = t, User = u })
            .GroupBy(m => new { UserName = m.User.Name, m.User.UserId })
            .Select(g => new { g.Key.UserId, Count = g.Count(), g.Key.UserName })
            .ToArrayAsync();

        var totalTaskCount = groupedBySource.Sum(g => g.Count);

        var sources = searchOptions.Sources ?? groupedBySource.Select(g => g.UserId).ToArray();
        var facets = new Dictionary<SupportTaskSearchFacet, IReadOnlyDictionary<object, int>>
        {
            [SupportTaskSearchFacet.Sources] = groupedBySource.ToDictionary(object (g) => new SupportTaskSource(g.UserId, g.UserName), g => g.Count)
        };

        var tasks = baseQuery.Join(dbContext.Persons, t =>
            t.TrnRequestMetadata!.ResolvedPersonId, p => p.PersonId, (t, p) => new { Task = t, Person = p });

        tasks = tasks.Where(t => sources.Contains(t.Task.TrnRequestMetadata!.ApplicationUserId));

        if (SearchTextIsName(search, out var nameParts))
        {
            tasks = tasks.Where(t =>
                nameParts.All(n => t.Task.TrnRequestMetadata!.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
        }

        var totalFilteredTaskCount = await tasks.CountAsync();

        tasks = sortBy switch
        {
            TrnRequestManualChecksSortByOption.Name => tasks
                .OrderBy(t => t.Task.TrnRequestMetadata!.FirstName, sortDirection)
                .ThenBy(t => t.Task.TrnRequestMetadata!.MiddleName, sortDirection)
                .ThenBy(t => t.Task.TrnRequestMetadata!.LastName, sortDirection),
            TrnRequestManualChecksSortByOption.DateOfBirth => tasks
                .OrderBy(t => t.Task.TrnRequestMetadata!.DateOfBirth, sortDirection),
            TrnRequestManualChecksSortByOption.Source => tasks
                .OrderBy(t => t.Task.TrnRequestMetadata!.ApplicationUser!.Name, sortDirection),
            _ => tasks
                .OrderBy(t => t.Task.CreatedOn, sortDirection),
        };

        var searchResults = await tasks
            .Select(t => t.Task)
            .Select(t => new TrnRequestManualChecksSearchResultItem(
                t.SupportTaskReference,
                t.TrnRequestMetadata!.FirstName!,
                t.TrnRequestMetadata!.MiddleName ?? "",
                t.TrnRequestMetadata!.LastName!,
                t.TrnRequestMetadata!.DateOfBirth,
                t.CreatedOn,
                t.TrnRequestMetadata.ApplicationUser!.Name))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.ItemsPerPage, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = totalTaskCount,
            Sources = sources,
            Facets = facets,
            SearchResults = searchResults
        };
    }

    public async Task<TeachersPensionsPotentialDuplicatesSearchResult> SearchTeachersPensionsPotentialDuplicatesAsync(TeachersPensionsPotentialDuplicatesSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var sortBy = searchOptions.SortBy ?? TeachersPensionsPotentialDuplicatesSortByOption.CreatedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = await dbContext.SupportTasks
            .Include(t => t.Person)
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && t.Status == SupportTaskStatus.Open)
            .ToListAsync();

        var unorderedResults = tasks.Select(t =>
        {
            var data = t.Data as TeacherPensionsPotentialDuplicateData;
            return new TeachersPensionsPotentialDuplicatesSearchResultItem
            (
                t.SupportTaskReference,
                data!.FileName,
                data!.IntegrationTransactionId,
                $"{t.Person!.FirstName} {t.Person!.LastName}",
                DateOnly.FromDateTime(t.CreatedOn)
            );
        }).AsQueryable();

        var searchResults = (sortBy switch
        {
            TeachersPensionsPotentialDuplicatesSortByOption.Name => unorderedResults
                .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase, sortDirection),
            TeachersPensionsPotentialDuplicatesSortByOption.Filename => unorderedResults
                .OrderBy(r => r.Filename, StringComparer.OrdinalIgnoreCase, sortDirection),
            TeachersPensionsPotentialDuplicatesSortByOption.InterfaceId => unorderedResults
                .OrderBy(r => r.IntegrationTransactionId, sortDirection),
            _ => unorderedResults
                .OrderBy(r => r.CreatedOn, sortDirection),
        }).GetPage(paginationOptions.PageNumber, paginationOptions.ItemsPerPage, tasks.Count);

        return new()
        {
            TotalTaskCount = tasks.Count,
            SearchResults = searchResults
        };
    }
    
    
    public IQueryable<SupportTask> SearchOneLoginIdVerificationSupportTasks(SearchOneLoginUserIdVerificationSupportTasksOptions options)
    {
        var query = dbContext.SupportTasks
            .Include(t => t.OneLoginUser)
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.Status == SupportTaskStatus.Open);

        query = options.SortBy switch
        {
            OneLoginIdVerificationSupportTasksSortByOption.SupportTaskReference => query.OrderBy(options.SortDirection, r => r.SupportTaskReference),
            OneLoginIdVerificationSupportTasksSortByOption.Name => query
                .OrderBy(options.SortDirection, r => (r.Data as OneLoginUserIdVerificationData)!.StatedFirstName)
                .ThenBy(options.SortDirection, r => (r.Data as OneLoginUserIdVerificationData)!.StatedLastName),
            OneLoginIdVerificationSupportTasksSortByOption.Email => query.OrderBy(options.SortDirection, r => r.OneLoginUser!.EmailAddress),
            OneLoginIdVerificationSupportTasksSortByOption.RequestedOn => query.OrderBy(options.SortDirection, r => r.CreatedOn),
            _ => query
        };

        return query;
    }

    private bool SearchTextIsDate(string searchText, out DateTime minDate, out DateTime maxDate)
    {
        DateOnly date;

        var isDate = DateOnly.TryParseExact(searchText, UiDefaults.DateOnlyDisplayFormat, out date) ||
            DateOnly.TryParseExact(searchText, "d/M/yyyy", out date);

        minDate = date.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
        maxDate = minDate.AddDays(1);

        return isDate;
    }

    private bool SearchTextIsEmailAddress(string searchText, out string email)
    {
        email = searchText;

        return searchText.Contains('@');
    }

    private bool SearchTextIsName(string searchText, out string[] nameParts)
    {
        nameParts = searchText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(n => n.ToLower(CultureInfo.InvariantCulture))
            .ToArray();

        return nameParts.Length > 0;
    }
}

public enum SupportTaskSearchFacet
{
    Sources
}

public record SupportTaskSource(Guid UserId, string UserName);

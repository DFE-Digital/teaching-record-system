using System.Globalization;
using System.Linq.Expressions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public class SupportTaskSearchService(TrsDbContext dbContext)
{
    private static readonly SupportTaskType[] _allChangeRequestTypes = [SupportTaskType.ChangeNameRequest, SupportTaskType.ChangeDateOfBirthRequest];

    public async Task<TrnRequestsSearchResult> SearchTrnRequestsAsync(TrnRequestsSearchOptions options, PaginationOptions paginationOptions)
    {
        var search = options.Search?.Trim() ?? string.Empty;
        var sortBy = options.SortBy ?? TrnRequestsSortByOption.RequestedOn;
        var sortDirection = options.SortDirection ?? SortDirection.Ascending;

        var tasks = dbContext.SupportTasks
            .Where(t => t.SupportTaskType == SupportTaskType.TrnRequest && t.IsOutstanding)
            .Join(
                dbContext.TrnRequestMetadata,
                t => new { ApplicationUserId = t.TrnRequestApplicationUserId!.Value, RequestId = t.TrnRequestId! },
                r => new { r.ApplicationUserId, r.RequestId },
                (t, r) => new { Task = t, TrnRequest = r })
            .Join(
                dbContext.ApplicationUsers,
                t => t.TrnRequest.ApplicationUserId,
                u => u.UserId,
                (t, user) => new { t.Task, t.TrnRequest, ApplicationUser = user });

        var totalTaskCount = await tasks.CountAsync();

        if (SearchTextIsDate(search, out var minDate, out var maxDate))
        {
            tasks = tasks.Where(t => t.Task.CreatedOn >= minDate && t.Task.CreatedOn < maxDate);
        }
        else if (SearchTextHelper.IsEmailAddress(search, out var email))
        {
            tasks = tasks.Where(t =>
                t.TrnRequest.EmailAddress != null &&
                EF.Functions.Collate(t.TrnRequest.EmailAddress, Collations.CaseInsensitive) == email);
        }
        else if (SearchTextIsName(search, out var nameParts))
        {
            tasks = tasks.Where(t =>
                nameParts.All(n => t.TrnRequest.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
        }

        var resultsBySourceApplication = await tasks
            .GroupBy(t => t.TrnRequest.ApplicationUserId)
            .Select(t => new TrnRequestsSearchResultBySourceApplication(t.Key, t.Count()))
            .ToArrayAsync();

        if (options.SourceApplicationUserIds.Count is not 0)
        {
            tasks = tasks.Where(t => options.SourceApplicationUserIds.Contains(t.TrnRequest.ApplicationUserId));
        }

        var totalFilteredTaskCount = await tasks.CountAsync();

        tasks = sortBy switch
        {
            TrnRequestsSortByOption.Name => tasks
                .OrderBy(t => t.TrnRequest.FirstName, sortDirection)
                .ThenBy(t => t.TrnRequest.MiddleName, sortDirection)
                .ThenBy(t => t.TrnRequest.LastName, sortDirection),
            TrnRequestsSortByOption.Email => tasks
                .OrderBy(t => t.TrnRequest.EmailAddress, sortDirection),
            TrnRequestsSortByOption.Source => tasks
                .OrderBy(t => t.TrnRequest.ApplicationUser!.Name, sortDirection),
            _ => tasks
                .OrderBy(t => t.Task.CreatedOn, sortDirection)
        };

        var searchResults = await tasks
            .Select(t => new TrnRequestsSearchResultItem(
                t.Task.SupportTaskReference,
                t.TrnRequest.FirstName!,
                t.TrnRequest.MiddleName ?? "",
                t.TrnRequest.LastName!,
                t.TrnRequest.EmailAddress,
                t.Task.CreatedOn,
                t.ApplicationUser.ShortName ?? t.ApplicationUser.Name))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.PageSize, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = totalTaskCount,
            SearchResults = searchResults,
            BySourceApplication = resultsBySourceApplication
        };
    }

    public async Task<ChangeRequestsSearchResult> SearchChangeRequestsAsync(ChangeRequestsSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? ChangeRequestsSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;
        var changeRequestTypes = searchOptions.ChangeRequestTypes ?? _allChangeRequestTypes;

        foreach (var t in changeRequestTypes)
        {
            if (!_allChangeRequestTypes.Contains(t))
            {
                throw new ArgumentOutOfRangeException($"Support task type {t} is not a Change Request task type.");
            }
        }

        var tasks = dbContext.SupportTasks
            .Include(t => t.Person)
            .Where(t => (t.SupportTaskType == SupportTaskType.ChangeNameRequest || t.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest)
                        && t.IsOutstanding);

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
                string.JoinNonEmpty(' ', t.Person.FirstName, t.Person.MiddleName, t.Person.LastName),
                t.CreatedOn,
                t.SupportTaskType))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.PageSize, totalFilteredTaskCount);

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
            .Where(t => t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded && t.IsOutstanding);

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
                t.TrnRequestMetadata.ApplicationUser!.ShortName ?? t.TrnRequestMetadata.ApplicationUser!.Name))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.PageSize, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = totalTaskCount,
            Sources = sources.AsReadOnly(),
            Facets = facets.AsReadOnly(),
            SearchResults = searchResults
        };
    }

    public async Task<TeachersPensionsPotentialDuplicatesSearchResult> SearchTeachersPensionsPotentialDuplicatesAsync(TeachersPensionsPotentialDuplicatesSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? TeachersPensionsPotentialDuplicatesSortByOption.CreatedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = await dbContext.SupportTasks
            .Include(t => t.Person)
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.TeacherPensionsPotentialDuplicate && t.IsOutstanding)
            .ToListAsync();

        var unorderedResults = tasks.Select(t =>
        {
            var data = t.Data as TeacherPensionsPotentialDuplicateData;
            return new TeachersPensionsPotentialDuplicatesSearchResultItem
            (
                t.SupportTaskReference,
                data!.FileName,
                data!.IntegrationTransactionId,
                string.JoinNonEmpty(' ', t.Person!.FirstName, t.Person!.MiddleName, t.Person!.LastName),
                t.CreatedOn,
                NameParts: new[] { t.Person!.FirstName, t.Person!.MiddleName, t.Person!.LastName }
            );
        }).AsQueryable();

        if (SearchTextIsDate(search, out var minDate, out var maxDate))
        {
            unorderedResults = unorderedResults.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextIsName(search, out var name))
        {
            unorderedResults = unorderedResults.Where(t =>
                name.All(n => t.NameParts!.Any(m => string.Equals(m, n, StringComparison.OrdinalIgnoreCase))));
        }

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
        }).GetPage(paginationOptions.PageNumber, paginationOptions.PageSize, tasks.Count);

        return new()
        {
            TotalTaskCount = tasks.Count,
            SearchResults = searchResults
        };
    }

    public async Task<OneLoginUserIdVerificationSupportTasksSearchResult> SearchOneLoginIdVerificationSupportTasksAsync(OneLoginUserIdVerificationSupportTasksOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? OneLoginUserIdVerificationSupportTasksSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = await dbContext.SupportTasks
            .Include(t => t.OneLoginUser)
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.IsOutstanding)
            .ToListAsync();

        var taskCount = tasks.Count;

        var clientUserIds = tasks
            .Select(t => ((OneLoginUserIdVerificationData)t.Data!).ClientApplicationUserId)
            .Distinct()
            .ToList();

        var users = await dbContext.ApplicationUsers
            .Where(u => clientUserIds.Contains(u.UserId))
            .Select(u => new
            {
                u.UserId,
                ShortName = (u.ShortName ?? u.Name)!
            })
            .ToDictionaryAsync(u => u.UserId, u => u.ShortName);

        var results = tasks
            .Select(r =>
            {
                var data = (OneLoginUserIdVerificationData)r.Data!;
                return new OneLoginUserIdVerificationSupportTasksSearchResultItem(
                    r.SupportTaskReference,
                    r.Status,
                    data.StatedFirstName,
                    data.StatedLastName,
                    r.OneLoginUser!.EmailAddress,
                    r.CreatedOn,
                    users[data.ClientApplicationUserId]!
                );
            })
            .AsQueryable();

        if (SearchTextIsDate(search, out var minDate, out var maxDate))
        {
            results = results.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextHelper.IsEmailAddress(search, out var email))
        {
            results = results.Where(t =>
                t.EmailAddress != null && string.Equals(t.EmailAddress, email, StringComparison.OrdinalIgnoreCase));
        }
        else if (SearchTextHelper.IsSupportTaskReference(search))
        {
            results = results.Where(t =>
                string.Equals(t.SupportTaskReference, search, StringComparison.OrdinalIgnoreCase));
        }
        else if (SearchTextIsName(search, out var nameParts))
        {
            results = results.Where(t =>
                nameParts.All(n => (new string[] { t.FirstName.ToLower(), t.LastName.ToLower() }).Contains(n.ToLower())));
        }

        var totalFilteredTaskCount = results.Count();

        var searchResults = (sortBy switch
        {
            OneLoginUserIdVerificationSupportTasksSortByOption.SupportTaskReference => results.OrderBy(r => r.SupportTaskReference, sortDirection),
            OneLoginUserIdVerificationSupportTasksSortByOption.Name => results
                .OrderBy(r => r.FirstName, sortDirection)
                .ThenBy(r => r.LastName, sortDirection),
            OneLoginUserIdVerificationSupportTasksSortByOption.Email => results.OrderBy(r => r.EmailAddress, sortDirection),
            OneLoginUserIdVerificationSupportTasksSortByOption.RequestedOn => results.OrderBy(r => r.CreatedOn, sortDirection).ThenBy(r => r.SupportTaskReference, sortDirection),
            OneLoginUserIdVerificationSupportTasksSortByOption.Source => results.OrderBy(r => r.SourceApplicationName, sortDirection),
            _ => results
        }).GetPage(paginationOptions.PageNumber, paginationOptions.PageSize, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = taskCount,
            SearchResults = searchResults
        };
    }

    public async Task<OneLoginUserRecordMatchingSupportTasksSearchResult> SearchOneLoginUserRecordMatchingSupportTasksAsync(OneLoginUserRecordMatchingSupportTasksOptions searchOptions, PaginationOptions paginationOptions)
    {
        var search = searchOptions.Search?.Trim() ?? string.Empty;
        var sortBy = searchOptions.SortBy ?? OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = await dbContext.SupportTasks
            .Include(t => t.OneLoginUser)
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserRecordMatching && t.IsOutstanding)
            .ToListAsync();

        var clientUserIds = tasks
            .Select(t => ((OneLoginUserRecordMatchingData)t.Data!).ClientApplicationUserId)
            .Distinct()
            .ToList();

        var users = await dbContext.ApplicationUsers.IgnoreQueryFilters()
            .Where(u => clientUserIds.Contains(u.UserId))
            .Select(u => new
            {
                u.UserId,
                ShortName = (u.ShortName ?? u.Name)!
            })
            .ToDictionaryAsync(u => u.UserId, u => u.ShortName);

        var taskCount = tasks.Count;

        var results = tasks
            .Select(r =>
            {
                var data = (r.Data as OneLoginUserRecordMatchingData)!;
                var name = users[(r.Data as OneLoginUserRecordMatchingData)!.ClientApplicationUserId];
                return new OneLoginUserRecordMatchingSupportTasksSearchResultItem(
                    r.SupportTaskReference,
                    r.Status,
                    data.VerifiedNames!.First().First(),
                    data.VerifiedNames!.First().Last(),
                    data.VerifiedNames!.Skip(1).SelectMany(n => n).ToArray(),
                    r.OneLoginUser!.EmailAddress,
                    r.CreatedOn,
                    name
                );
            })
            .AsQueryable();

        if (SearchTextIsDate(search, out var minDate, out var maxDate))
        {
            results = results.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextHelper.IsEmailAddress(search, out var email))
        {
            results = results.Where(t =>
                t.EmailAddress != null && string.Equals(t.EmailAddress, email, StringComparison.OrdinalIgnoreCase));
        }
        else if (SearchTextHelper.IsSupportTaskReference(search))
        {
            results = results.Where(t =>
                string.Equals(t.SupportTaskReference, search, StringComparison.OrdinalIgnoreCase));
        }
        else if (SearchTextIsName(search, out var nameParts))
        {
            results = results.Where(t =>
                nameParts.All(n => (new[] { t.FirstName.ToLower(), t.LastName.ToLower() }.Concat(t.OtherVerifiedNames.Select(ovn => ovn.ToLower()))).Contains(n.ToLower())));
        }

        var totalFilteredTaskCount = results.Count();

        var searchResults = (sortBy switch
        {
            OneLoginUserRecordMatchingSupportTasksSortByOption.SupportTaskReference => results.OrderBy(r => r.SupportTaskReference, sortDirection),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Name => results
                .OrderBy(r => r.FirstName, sortDirection)
                .ThenBy(r => r.LastName, sortDirection),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Email => results.OrderBy(r => r.EmailAddress, sortDirection),
            OneLoginUserRecordMatchingSupportTasksSortByOption.RequestedOn => results.OrderBy(r => r.CreatedOn, sortDirection).ThenBy(r => r.SupportTaskReference, sortDirection),
            OneLoginUserRecordMatchingSupportTasksSortByOption.Source => results.OrderBy(r => r.SourceApplicationName, sortDirection),
            _ => results
        }).GetPage(paginationOptions.PageNumber, paginationOptions.PageSize, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = taskCount,
            SearchResults = searchResults
        };
    }

    public async Task<SupportTasksSearchResult> SearchSupportTasksAsync(SupportTasksSearchOptions searchOptions, PaginationOptions paginationOptions)
    {
        var sortBy = searchOptions.SortBy ?? SupportTasksSortByOption.RequestedOn;
        var sortDirection = searchOptions.SortDirection ?? SortDirection.Ascending;

        var tasks = dbContext.SupportTasks.AsQueryable();

        if (searchOptions.SupportTaskType is { } supportTaskType)
        {
            tasks = tasks.Where(t => t.SupportTaskType == supportTaskType);
        }

        if (searchOptions.AssignedToUserId is { } assignedToUserId)
        {
            tasks = tasks.Where(t => t.AssignedToUserId == assignedToUserId);
        }

        if (searchOptions.Statuses is { Count: not 0 } statuses)
        {
            tasks = tasks.Where(t => statuses.Contains(t.Status));
        }

        var totalFilteredTaskCount = await tasks.CountAsync();

        tasks = sortBy switch
        {
            SupportTasksSortByOption.Subject => tasks
                .OrderBy(t => t.SubjectName ?? t.SubjectEmailAddress, sortDirection),
            SupportTasksSortByOption.TaskType => tasks
                .OrderBy(GetOrderByTypeExpression(), sortDirection),
            SupportTasksSortByOption.Status => tasks
                .OrderBy(t => t.Status, sortDirection),
            SupportTasksSortByOption.AssignedTo => tasks
                .OrderBy(t => t.AssignedTo!.Name, sortDirection),
            SupportTasksSortByOption.RequestedOn => tasks
                .OrderBy(t => t.CreatedOn, sortDirection),
            _ => tasks
                .OrderBy(t => t.SupportTaskReference, sortDirection)
        };

        var searchResults = await tasks
            .Select(t => new SupportTasksSearchResultItem(
                t.SupportTaskReference,
                (t.SubjectName ?? t.SubjectEmailAddress)!,
                t.SupportTaskType,
                t.Status,
                t.AssignedToUserId,
                t.AssignedTo != null ? t.AssignedTo.Name : null,
                t.CreatedOn))
            .GetPageAsync(paginationOptions.PageNumber, paginationOptions.PageSize, totalFilteredTaskCount);

        return new()
        {
            TotalTaskCount = totalFilteredTaskCount,
            SearchResults = searchResults
        };

        Expression<Func<SupportTask, int>> GetOrderByTypeExpression()
        {
            var typesOrderedByTitle = SupportTaskTypeRegistry.All
                .OrderBy(t => t.Title)
                .Select(t => (int)t.SupportTaskType)
                .ToArray();

            var parameter = Expression.Parameter(typeof(SupportTask), "t");
            var typeAsInt = Expression.Convert(
                Expression.Property(parameter, nameof(SupportTask.SupportTaskType)),
                typeof(int));

            // Build a CASE expression that maps each task type to its position in typesOrderedByTitle.
            Expression body = Expression.Constant(typesOrderedByTitle.Length);
            for (var i = typesOrderedByTitle.Length - 1; i >= 0; i--)
            {
                body = Expression.Condition(
                    Expression.Equal(typeAsInt, Expression.Constant(typesOrderedByTitle[i])),
                    Expression.Constant(i),
                    body);
            }

            return Expression.Lambda<Func<SupportTask, int>>(body, parameter);
        }
    }

    private bool SearchTextIsDate(string searchText, out DateTime minDate, out DateTime maxDate)
    {
        if (SearchTextHelper.IsDate(searchText, out var date))
        {
            minDate = date.ToDateTime(new TimeOnly(0, 0), DateTimeKind.Utc);
            maxDate = minDate.AddDays(1);
            return true;
        }

        minDate = default;
        maxDate = default;
        return false;
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

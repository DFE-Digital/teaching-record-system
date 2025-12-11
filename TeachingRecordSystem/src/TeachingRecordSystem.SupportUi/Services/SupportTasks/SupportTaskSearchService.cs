using System.Globalization;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Services.SupportTasks;

public class SupportTaskSearchService(TrsDbContext dbContext)
{
    public IQueryable<SupportTask> SearchApiTrnRequests(SearchApiTrnRequestsOptions options)
    {
        var tasks = dbContext.SupportTasks
            .Include(t => t.TrnRequestMetadata)
            .ThenInclude(m => m!.ApplicationUser)
            .Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest && t.Status == SupportTaskStatus.Open);

        var search = options.Search?.Trim() ?? string.Empty;

        if (SearchTextIsDate(out var date))
        {
            var minDate = date.ToDateTime(new TimeOnly(0, 0, 0), DateTimeKind.Utc);
            var maxDate = minDate.AddDays(1);
            tasks = tasks.Where(t => t.CreatedOn >= minDate && t.CreatedOn < maxDate);
        }
        else if (SearchTextIsEmailAddress())
        {
            tasks = tasks.Where(t =>
                t.TrnRequestMetadata!.EmailAddress != null && EF.Functions.Collate(t.TrnRequestMetadata.EmailAddress, Collations.CaseInsensitive) == search);
        }
        else
        {
            var nameParts = search
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(n => n.ToLower(CultureInfo.InvariantCulture))
                .ToArray();

            if (nameParts.Length > 0)
            {
                tasks = tasks.Where(t =>
                    nameParts.All(n => t.TrnRequestMetadata!.Name.Select(m => EF.Functions.Collate(m, Collations.CaseInsensitive)).Contains(n)));
            }
        }

        if (options.SortBy == ApiTrnRequestsSortByOption.Name)
        {
            tasks = tasks
                .OrderBy(options.SortDirection, t => t.TrnRequestMetadata!.FirstName)
                .ThenBy(options.SortDirection, t => t.TrnRequestMetadata!.MiddleName)
                .ThenBy(options.SortDirection, t => t.TrnRequestMetadata!.LastName);
        }
        else if (options.SortBy == ApiTrnRequestsSortByOption.Email)
        {
            tasks = tasks.OrderBy(options.SortDirection, t => t.TrnRequestMetadata!.EmailAddress);
        }
        else if (options.SortBy == ApiTrnRequestsSortByOption.RequestedOn)
        {
            tasks = tasks.OrderBy(options.SortDirection, t => t.CreatedOn);
        }
        else if (options.SortBy == ApiTrnRequestsSortByOption.Source)
        {
            tasks = tasks.OrderBy(options.SortDirection, t => t.TrnRequestMetadata!.ApplicationUser!.Name);
        }

        return tasks;

        bool SearchTextIsDate(out DateOnly date) =>
            DateOnly.TryParseExact(search, UiDefaults.DateOnlyDisplayFormat, out date) ||
            DateOnly.TryParseExact(search, "d/M/yyyy", out date);

        bool SearchTextIsEmailAddress() => search.Contains('@');
    }

    public IQueryable<SupportTask> SearchOneLoginIdVerificationSupportTasks(SearchOneLoginUserIdVerificationRequestsOptions options)
    {
        var query = dbContext.SupportTasks
            .Include(t => t.OneLoginUser)
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.Status == SupportTaskStatus.Open);

        query = options.SortBy switch
        {
            OneLoginIdVerificationRequestsSortByOption.ReferenceId => query.OrderBy(options.SortDirection, r => r.SupportTaskReference),
            OneLoginIdVerificationRequestsSortByOption.Name => query
                .OrderBy(options.SortDirection, r => (r.Data as OneLoginUserIdVerificationData)!.StatedFirstName)
                .ThenBy(options.SortDirection, r => (r.Data as OneLoginUserIdVerificationData)!.StatedLastName),
            OneLoginIdVerificationRequestsSortByOption.Email => query.OrderBy(options.SortDirection, r => r.OneLoginUser!.EmailAddress),
            OneLoginIdVerificationRequestsSortByOption.RequestedOn => query.OrderBy(options.SortDirection, r => r.CreatedOn),
            _ => query
        };

        return query;
    }
}

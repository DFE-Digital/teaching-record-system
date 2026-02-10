using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.Persons;

[RedactParameters("Search")]
public class IndexModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int PageSize = 15;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Sort by")]
    public PersonSearchSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public SortDirection? SortDirection { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IncludeActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IncludeDeactivated { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IncludeOneLoginUser { get; set; }

    public ResultPage<PersonInfo>? SearchResults { get; set; }

    public PaginationViewModel? Pagination { get; set; }

    public IReadOnlyDictionary<string, IReadOnlyDictionary<object, int>>? Facets { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Search = Search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(Search))
        {
            return Redirect(linkGenerator.Index());
        }

        var sortBy = SortBy ?? PersonSearchSortByOption.Name;
        var sortDirection = SortDirection ?? TeachingRecordSystem.SupportUi.SortDirection.Ascending;

        IQueryable<Person> query;

        if (SearchTextIsEmail())
        {
            var oneLoginUser = await dbContext.OneLoginUsers
                .Where(o => o.EmailAddress == Search && o.PersonId != null)
                .FirstOrDefaultAsync();

            if (oneLoginUser is null)
            {
                SearchResults = new ResultPage<PersonInfo>([], 0, PageSize, PageNumber ?? 1);
                Pagination = PaginationViewModel.Create(
                    SearchResults,
                    pageNumber => linkGenerator.Persons.Index(Search, IncludeActive, IncludeDeactivated, IncludeOneLoginUser, SortBy, SortDirection, pageNumber));
                return Page();
            }

            query = dbContext.Persons
                .IgnoreQueryFilters()
                .Where(p => p.PersonId == oneLoginUser.PersonId);
        }
        else if (SearchTextIsDate(out var dateOfBirth))
        {
            query = dbContext.Persons.IgnoreQueryFilters().Where(p => p.DateOfBirth == dateOfBirth);
        }
        else if (SearchTextIsTrn())
        {
            query = dbContext.Persons.IgnoreQueryFilters().Where(p => p.Trn == Search);
        }
        else if (!string.IsNullOrWhiteSpace(Search))
        {
            query = dbContext.Persons.FromSqlRaw(
                    """
                    SELECT * FROM persons
                    WHERE fn_split_names(ARRAY[:names]::varchar[]) COLLATE "case_insensitive" <@ names
                    """,
                    parameters:
                    [
                        // ReSharper disable once FormatStringProblem
                        new NpgsqlParameter("names", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = Search }
                    ])
                .IgnoreQueryFilters();
        }
        else
        {
            query = dbContext.Persons.IgnoreQueryFilters();
        }

        // Include OneLoginUsers for facet counting and filtering
        query = query.Include(p => p.OneLoginUsers);

        var groupedByStatus = await query
            .Select(p => p.Status)
            .GroupBy(p => p)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToArrayAsync();

        var hasOneLoginUserCount = await query
            .Where(p => p.OneLoginUsers != null && p.OneLoginUsers.Any())
            .CountAsync();

        Facets = new Dictionary<string, IReadOnlyDictionary<object, int>>
        {
            [nameof(IncludeActive)] = new Dictionary<object, int> { [true] = groupedByStatus.FirstOrDefault(g => g.Status == PersonStatus.Active)?.Count ?? 0 },
            [nameof(IncludeDeactivated)] = new Dictionary<object, int> { [true] = groupedByStatus.FirstOrDefault(g => g.Status == PersonStatus.Deactivated)?.Count ?? 0 },
            [nameof(IncludeOneLoginUser)] = new Dictionary<object, int> { [true] = hasOneLoginUserCount }
        };

        // Default to showing active records if no status filters are specified
        if (IncludeActive is null && IncludeDeactivated is null && IncludeOneLoginUser is null)
        {
            IncludeActive = true;
        }

        // Apply status filtering
        var statusesToInclude = new List<PersonStatus>();
        if (IncludeActive == true)
        {
            statusesToInclude.Add(PersonStatus.Active);
        }
        if (IncludeDeactivated == true)
        {
            statusesToInclude.Add(PersonStatus.Deactivated);
        }

        if (statusesToInclude.Count > 0)
        {
            query = query.Where(p => statusesToInclude.Contains(p.Status));
        }

        if (IncludeOneLoginUser == true)
        {
            query = query.Where(p => p.OneLoginUsers != null && p.OneLoginUsers.Any());
        }

        var totalPersonCount = await query.CountAsync();

        // Apply sorting
        IOrderedQueryable<Person> orderedQuery = sortBy switch
        {
            PersonSearchSortByOption.Name => query
                .OrderBy(p => p.FirstName, sortDirection)
                .ThenBy(p => p.MiddleName, sortDirection)
                .ThenBy(p => p.LastName, sortDirection),
            PersonSearchSortByOption.DateOfBirth => query
                .OrderBy(p => p.DateOfBirth, sortDirection),
            PersonSearchSortByOption.OneLoginEmailAddress => query
                .OrderBy(p => p.OneLoginUsers!.OrderBy(o => o.EmailAddress).Select(o => o.EmailAddress).FirstOrDefault(), sortDirection),
            PersonSearchSortByOption.Trn => query
                .OrderBy(p => p.Trn, sortDirection),
            PersonSearchSortByOption.NationalInsuranceNumber => query
                .OrderBy(p => p.NationalInsuranceNumber, sortDirection),
            PersonSearchSortByOption.RecordStatus => query
                .OrderBy(p => p.Status, sortDirection),
            _ => query
                .OrderBy(p => p.FirstName, sortDirection)
                .ThenBy(p => p.MiddleName, sortDirection)
                .ThenBy(p => p.LastName, sortDirection)
        };

        SearchResults = await orderedQuery
            .AsSplitQuery()
            .Select(p => new PersonInfo
            {
                PersonId = p.PersonId,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                Trn = p.Trn,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                PersonStatus = p.Status,
                OneLoginUserEmailAddresses = p.OneLoginUsers != null
                    ? p.OneLoginUsers.Select(o => o.EmailAddress!).Where(e => e != null).ToArray()!
                    : Array.Empty<string>()
            })
            .GetPageAsync(PageNumber, PageSize, totalPersonCount);

        Pagination = PaginationViewModel.Create(
            SearchResults!,
            pageNumber => linkGenerator.Persons.Index(Search, IncludeActive, IncludeDeactivated, IncludeOneLoginUser, SortBy, SortDirection, pageNumber));

        return Page();

        bool SearchTextIsDate(out DateOnly date) =>
            DateOnly.TryParseExact(Search, WebConstants.DateOnlyDisplayFormat, out date) ||
            DateOnly.TryParseExact(Search, "d/M/yyyy", out date);

        bool SearchTextIsTrn() => Search.Length == 7 && Search.All(Char.IsAsciiDigit);

        bool SearchTextIsEmail() => EmailAddress.TryParse(Search, out _);
    }

    public record PersonInfo
    {
        public required Guid PersonId { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required PersonStatus PersonStatus { get; init; }
        public required string[] OneLoginUserEmailAddresses { get; init; }

        public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);
    }

#pragma warning disable IDE1006 // Naming Styles
    private record NameSearchQueryResult(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}

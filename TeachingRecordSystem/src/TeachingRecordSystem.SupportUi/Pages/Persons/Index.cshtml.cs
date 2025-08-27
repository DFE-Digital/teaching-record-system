using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared;

namespace TeachingRecordSystem.SupportUi.Pages.Persons;

[RedactParameters("Search")]
public class IndexModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    private const int PageSize = 15;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Sort by")]
    public PersonSearchSortByOption? SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public PersonStatus[]? Statuses { get; set; }

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

        var sortBy = SortBy ??= PersonSearchSortByOption.LastNameAscending;

        var query = dbContext.Persons.IgnoreQueryFilters();

        if (SearchTextIsDate(out var dateOfBirth))
        {
            query = query.Where(p => p.DateOfBirth == dateOfBirth);
        }
        else if (SearchTextIsTrn())
        {
            query = query.Where(p => p.Trn == Search);
        }
        else
        {
            var nameParts = Search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (nameParts.Length > 0)
            {
                query = dbContext.Database.SqlQueryRaw<NameSearchQueryResult>(
                        """
                        select a.person_id from (
                            select unnest(:names) collate "case_insensitive" as name
                        ) x, person_search_attributes a
                        where a.attribute_value = x.name
                        and a.attribute_type in ('FirstName', 'MiddleName', 'LastName')
                        group by a.person_id
                        having count(distinct x.name) = array_length(:names, 1)
                        """,
                        parameters:
                        // ReSharper disable once FormatStringProblem
                        [
                            new NpgsqlParameter("names", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Varchar)
                            {
                                Value = nameParts
                            }
                        ])
                    .Join(query, r => r.person_id, p => p.PersonId, (_, p) => p);
            }
        }

        var groupedByStatus = await query
            .Select(p => p.Status)
            .GroupBy(p => p)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToArrayAsync();

        Facets = new Dictionary<string, IReadOnlyDictionary<object, int>>
        {
            [nameof(Statuses)] = groupedByStatus.ToDictionary(g => (object)g.Status, g => g.Count)
        };

        if (Statuses is [] or null)
        {
            Statuses = [PersonStatus.Active];
        }

        if (Statuses?.Order().ToArray() is not [PersonStatus.Active, PersonStatus.Deactivated])
        {
            query = query.Where(p => Statuses!.Contains(p.Status));
        }

        var totalPersonCount = await query.CountAsync();

        if (sortBy == PersonSearchSortByOption.LastNameAscending)
        {
            query = query.OrderBy(p => p.LastName);
        }
        else if (sortBy == PersonSearchSortByOption.LastNameDescending)
        {
            query = query.OrderByDescending(p => p.LastName);
        }
        else if (sortBy == PersonSearchSortByOption.FirstNameAscending)
        {
            query = query.OrderBy(p => p.FirstName);
        }
        else if (sortBy == PersonSearchSortByOption.FirstNameDescending)
        {
            query = query.OrderByDescending(p => p.FirstName);
        }
        else if (sortBy == PersonSearchSortByOption.DateOfBirthAscending)
        {
            query = query.OrderBy(p => p.DateOfBirth);
        }
        else
        {
            Debug.Assert(sortBy == PersonSearchSortByOption.DateOfBirthDescending);
            query = query.OrderByDescending(p => p.DateOfBirth);
        }

        SearchResults = await query
            .Select(p => new PersonInfo
            {
                PersonId = p.PersonId,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                Trn = p.Trn,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                PersonStatus = p.Status
            })
            .GetPageAsync(PageNumber, PageSize, totalPersonCount);

        Pagination = PaginationViewModel.Create(
            SearchResults!,
            pageNumber => linkGenerator.Persons(Search, Statuses, SortBy, pageNumber));

        return Page();

        bool SearchTextIsDate(out DateOnly date) =>
            DateOnly.TryParseExact(Search, UiDefaults.DateOnlyDisplayFormat, out date) ||
            DateOnly.TryParseExact(Search, "d/M/yyyy", out date);

        bool SearchTextIsTrn() => Search.Length == 7 && Search.All(Char.IsAsciiDigit);
    }

    public record PersonInfo
    {
        public required Guid PersonId { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required PersonStatus PersonStatus { get; init; }

        public string Name => StringHelper.JoinNonEmpty(' ', FirstName, MiddleName, LastName);
    }

#pragma warning disable IDE1006 // Naming Styles
    private record NameSearchQueryResult(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}

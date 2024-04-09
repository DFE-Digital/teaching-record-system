using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons;

public partial class IndexModel : PageModel
{
    private const int MaxSearchResultCount = 500;
    private const int PageSize = 15;

    [GeneratedRegex("^\\d{7}$")]
    private static partial Regex TrnRegex();

    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Search", Description = "TRN, name or date of birth, for example 4/3/1975")]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Sort by")]
    public ContactSearchSortByOption SortBy { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int[]? PaginationPages { get; set; }

    public int TotalKnownPages { get; set; }

    public bool DisplayPageNumbers { get; set; }

    public int? PreviousPage { get; set; }

    public int? NextPage { get; set; }

    public PersonInfo[]? SearchResults { get; set; }

    public async Task<IActionResult> OnGet()
    {
        PageNumber ??= 1;

        if (!string.IsNullOrEmpty(Search))
        {
            if (PageNumber < 1)
            {
                return BadRequest();
            }

            return await PerformSearch();
        }

        return Page();
    }

    private async Task<IActionResult> PerformSearch()
    {
        Contact[]? contacts = null;

        var columnSet = new ColumnSet(
            Contact.Fields.dfeta_TRN,
            Contact.Fields.BirthDate,
            Contact.Fields.FirstName,
            Contact.Fields.MiddleName,
            Contact.Fields.LastName,
            Contact.Fields.FullName,
            Contact.Fields.dfeta_StatedFirstName,
            Contact.Fields.dfeta_StatedMiddleName,
            Contact.Fields.dfeta_StatedLastName,
            Contact.Fields.dfeta_NINumber);

        // Check if the search string is a date of birth, TRN or one or more names
        if (DateOnly.TryParse(Search, out var dateOfBirth))
        {
            contacts = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactsByDateOfBirthQuery(dateOfBirth, SortBy, MaxSearchResultCount, columnSet));
        }
        else if (TrnRegex().IsMatch(Search!))
        {
            var contact = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactByTrnQuery(Search!, columnSet));
            contacts = contact is not null ? [contact] : [];
        }
        else
        {
            contacts = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactsByNameQuery(Search!, SortBy, MaxSearchResultCount, columnSet));
        }
        Debug.Assert(contacts is not null);

        TotalKnownPages = Math.Max((int)Math.Ceiling((decimal)contacts!.Length / PageSize), 1);

        PreviousPage = PageNumber > 1 ? PageNumber - 1 : null;
        NextPage = PageNumber < TotalKnownPages ? PageNumber + 1 : null;

        if (contacts.Length < MaxSearchResultCount)
        {
            DisplayPageNumbers = true;
            PaginationPages = Enumerable.Range(-2, 5).Select(offset => PageNumber!.Value + offset)
                .Append(1)
                .Append(TotalKnownPages)
                .Where(page => page <= TotalKnownPages && page >= 1)
                .Distinct()
                .Order()
                .ToArray();
        }

        SearchResults = contacts
            .Skip((PageNumber!.Value - 1) * PageSize)
            .Take(PageSize)
            .Select(MapContact)
            .ToArray();

        // if the page number is greater than the total number of pages, redirect to the first page
        if (PageNumber > TotalKnownPages)
        {
            return Redirect(_linkGenerator.Persons(search: Search));
        }

        return Page();
    }

    private PersonInfo MapContact(Contact contact)
    {
        return new PersonInfo()
        {
            PersonId = contact.ContactId!.Value,
            Name = contact.ResolveFullName(includeMiddleName: true),
            DateOfBirth = contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            Trn = contact.dfeta_TRN,
            NationalInsuranceNumber = contact.dfeta_NINumber
        };
    }

    public record PersonInfo
    {
        public required Guid PersonId { get; set; }
        public required string Name { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
    }
}

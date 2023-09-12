using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons;

public partial class IndexModel : PageModel
{
    private const int MaxSearchResultCount = 500;
    private const int PageSize = 25;

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
    [Display(Name = "Search")]
    public string? Search { get; set; }

    [FromQuery(Name = "PageNumber")]
    public int? PageNumber { get; set; }

    public PersonInfo[]? SearchResults { get; set; }

    public int TotalKnownPages { get; set; }

    public int? PreviousPage { get; set; }

    public int? NextPage { get; set; }

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

    public async Task<IActionResult> OnPost()
    {
        PageNumber ??= 1;

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (string.IsNullOrEmpty(Search))
        {
            ModelState.AddModelError(nameof(Search), "Enter search string");
            return this.PageWithErrors();
        }

        return await PerformSearch();
    }

    private async Task<IActionResult> PerformSearch()
    {
        var contacts = new Contact[] { };
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
            Contact.Fields.EMailAddress1,
            Contact.Fields.MobilePhone,
            Contact.Fields.dfeta_NINumber,
            Contact.Fields.dfeta_ActiveSanctions);

        // Check if the search string is a date of birth, TRN or one or more names
        if (DateOnly.TryParse(Search, out var dateOfBirth))
        {
            contacts = await _crmQueryDispatcher.ExecuteQuery(new GetContactsByDateOfBirthQuery(dateOfBirth, MaxSearchResultCount, columnSet));
        }
        else if (TrnRegex().IsMatch(Search!))
        {
            var contact = await _crmQueryDispatcher.ExecuteQuery(new GetContactByTrnQuery(Search!, columnSet));
            if (contact != null)
            {
                contacts = new[] { contact };
            }
        }
        else
        {
            contacts = await _crmQueryDispatcher.ExecuteQuery(new GetContactsByNameQuery(Search!, MaxSearchResultCount, columnSet));
        }

        TotalKnownPages = Math.Max((int)Math.Ceiling((decimal)contacts.Length / PageSize), 1);

        PreviousPage = PageNumber > 1 ? PageNumber - 1 : null;
        NextPage = PageNumber < TotalKnownPages ? PageNumber + 1 : null;

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
            NationalInsuranceNumber = contact.dfeta_NINumber,
            HasActiveAlert = contact.dfeta_ActiveSanctions ?? false
        };
    }

    public record PersonInfo
    {
        public required Guid PersonId { get; set; }
        public required string Name { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required bool HasActiveAlert { get; init; }
    }
}

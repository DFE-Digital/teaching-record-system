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
    [GeneratedRegex("^\\d{7}$")]
    private static partial Regex TrnRegex();
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [BindProperty]
    [Display(Name = "Search")]
    [Required(ErrorMessage = "Enter search criteria")]
    public string? Search { get; set; }

    public PersonInfo[]? SearchResults { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

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
            contacts = await _crmQueryDispatcher.ExecuteQuery(new GetContactsByDateOfBirthQuery(dateOfBirth, columnSet));

        }
        else if (TrnRegex().IsMatch(Search))
        {
            var contact = await _crmQueryDispatcher.ExecuteQuery(new GetContactByTrnQuery(Search!, columnSet));
            if (contact != null)
            {
                contacts = new[] { contact };
            }
        }
        else
        {
            contacts = await _crmQueryDispatcher.ExecuteQuery(new GetContactsByNameQuery(Search!, columnSet));
        }

        SearchResults = contacts.Select(MapContact).ToArray();

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

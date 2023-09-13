using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class IndexModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public PersonSubNavigationTab? SelectedTab { get; set; }

    public PersonInfo? Person { get; set; }

    public async Task<IActionResult> OnGet()
    {
        SelectedTab ??= PersonSubNavigationTab.General;

        var contactDetail = await _crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                PersonId,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.BirthDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.MobilePhone,
                    Contact.Fields.dfeta_NINumber)));

        if (contactDetail is null)
        {
            return NotFound();
        }

        Person = MapContact(contactDetail.Contact);

        return Page();
    }

    private PersonInfo MapContact(Contact contact)
    {
        return new PersonInfo()
        {
            Name = contact.ResolveFullName(includeMiddleName: false),
            FullName = contact.ResolveFullName(includeMiddleName: true),
            DateOfBirth = contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            Trn = contact.dfeta_TRN,
            NationalInsuranceNumber = contact.dfeta_NINumber,
            Email = contact.EMailAddress1,
            MobileNumber = contact.MobilePhone
        };
    }

    public record PersonInfo
    {
        public required string Name { get; init; }
        public required string FullName { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required string? Email { get; init; }
        public required string? MobileNumber { get; init; }
    }

    public enum PersonSubNavigationTab
    {
        General,
        Alerts,
        ChangeLog
    }
}

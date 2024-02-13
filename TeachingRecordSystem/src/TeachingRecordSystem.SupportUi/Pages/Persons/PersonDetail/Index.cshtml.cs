using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class IndexModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    IConfiguration configuration) : PageModel
{
    private readonly TimeSpan _concurrentNameChangeWindow = TimeSpan.FromSeconds(configuration.GetValue<int>("ConcurrentNameChangeWindowSeconds", 5));

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

    public PersonInfo? Person { get; set; }

    public async Task OnGet()
    {
        var contactDetail = await crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                PersonId,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.CreatedOn,
                    Contact.Fields.BirthDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.MobilePhone,
                    Contact.Fields.dfeta_NINumber,
                    Contact.Fields.GenderCode,
                    Contact.Fields.dfeta_ActiveSanctions)));

        var contact = contactDetail!.Contact;
        var previousNames = PreviousNameHelper.GetFullPreviousNames(contactDetail.PreviousNames, contactDetail.Contact, _concurrentNameChangeWindow);

        Person = new PersonInfo()
        {
            Name = contact.ResolveFullName(includeMiddleName: true),
            DateOfBirth = contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            Trn = contact.dfeta_TRN,
            NationalInsuranceNumber = contact.dfeta_NINumber,
            Email = contact.EMailAddress1,
            MobileNumber = contact.MobilePhone,
            Gender = contact.GenderCode.ToString(),
            HasAlerts = contact.dfeta_ActiveSanctions == true,
            PreviousNames = previousNames
                .Select(name => GetFullName(name.FirstName, name.MiddleName, name.LastName))
                .ToArray()
        };
    }

    private static string GetFullName(string? firstName, string? middleName, string? lastName)
    {
        var fullName = new StringBuilder(firstName);
        if (!string.IsNullOrEmpty(middleName))
        {
            if (fullName.Length > 0)
            {
                fullName.Append(' ');
            }

            fullName.Append(middleName);
        }

        if (!string.IsNullOrEmpty(lastName))
        {
            if (fullName.Length > 0)
            {
                fullName.Append(' ');
            }

            fullName.Append(lastName);
        }

        return fullName.ToString();
    }

    public record PersonInfo
    {
        public required string Name { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required string? Email { get; init; }
        public required string? MobileNumber { get; init; }
        public required string? Gender { get; init; }
        public required bool HasAlerts { get; init; }
        public required string[] PreviousNames { get; init; }
    }
}

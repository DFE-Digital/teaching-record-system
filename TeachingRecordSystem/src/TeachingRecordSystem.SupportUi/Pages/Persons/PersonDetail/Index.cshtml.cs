using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[AllowDeactivatedPerson]
public class IndexModel(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    PreviousNameHelper previousNameHelper,
    IFeatureProvider featureProvider) : PageModel
{
    private static readonly InductionStatus[] _invalidInductionStatusesForMerge = [InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed];

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public PersonSearchSortByOption? SortBy { get; set; }

    public PersonInfo? Person { get; set; }

    public PersonProfessionalStatusInfo? PersonProfessionalStatus { get; set; }

    public bool HasOpenAlert { get; set; }

    public bool ShowMergeButton { get; set; }

    public async Task OnGetAsync()
    {
        HasOpenAlert = await dbContext.Alerts.AnyAsync(a => a.PersonId == PersonId && a.IsOpen);

        Person = await BuildPersonInfoAsync();
        PersonProfessionalStatus = await BuildPersonStatusInfoAsync();

        ShowMergeButton = !HasOpenAlert && Person!.IsActive &&
            (PersonProfessionalStatus is not PersonProfessionalStatusInfo professionalStatus || !_invalidInductionStatusesForMerge.Contains(professionalStatus.InductionStatus));
    }

    private async Task<PersonProfessionalStatusInfo?> BuildPersonStatusInfoAsync()
    {
        var person = await dbContext.Persons
            .IgnoreQueryFilters()
            .SingleAsync(p => p.PersonId == PersonId);

        var personProfessionalStatusInfo = new PersonProfessionalStatusInfo
        {
            EytsDate = person.EytsDate,
            HasEyps = person.HasEyps,
            InductionStatus = person.InductionStatus,
            PqtsDate = person.PqtsDate,
            QtsDate = person.QtsDate,
            QtlsStatus = person.QtlsStatus
        };

        return personProfessionalStatusInfo is
        {
            EytsDate: null,
            HasEyps: false,
            InductionStatus: InductionStatus.None,
            PqtsDate: null,
            QtsDate: null,
            QtlsStatus: QtlsStatus.None
        } ? null : personProfessionalStatusInfo;
    }

    private async Task<PersonInfo> BuildPersonInfoAsync()
    {
        var hasActiveAlert = await dbContext.Alerts.Where(a => a.PersonId == PersonId && a.IsOpen).AnyAsync();
        if (featureProvider.IsEnabled(FeatureNames.ContactsMigrated))
        {
            var person = await dbContext.Persons
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(p => p.PersonId == PersonId);

            var previousNames = await dbContext.PreviousNames
                .Where(n => n.PersonId == PersonId)
                .OrderByDescending(n => n.CreatedOn)
                .ToArrayAsync();

            return new PersonInfo
            {
                Name = StringHelper.JoinNonEmpty(' ', person!.FirstName, person.MiddleName, person.LastName),
                DateOfBirth = person.DateOfBirth,
                Trn = person.Trn,
                NationalInsuranceNumber = person.NationalInsuranceNumber,
                Email = person.EmailAddress,
                MobileNumber = person.MobileNumber,
                Gender = person.Gender,
                HasActiveAlert = hasActiveAlert,
                PreviousNames = previousNames
                    .Select(name => StringHelper.JoinNonEmpty(' ', name.FirstName, name.MiddleName, name.LastName))
                    .ToArray(),
                IsActive = person.Status == PersonStatus.Active
            };
        }
        else
        {
            var contactDetail = await crmQueryDispatcher.ExecuteQueryAsync(
                new GetActiveContactDetailByIdQuery(
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
                        Contact.Fields.StatusCode)));
            var contact = contactDetail!.Contact;

            return new PersonInfo()
            {
                Name = contact.ResolveFullName(includeMiddleName: true),
                DateOfBirth = contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                Trn = contact.dfeta_TRN,
                NationalInsuranceNumber = contact.dfeta_NINumber,
                Email = contact.EMailAddress1,
                MobileNumber = contact.MobilePhone,
                Gender = contact.GenderCode.ToGender(),
                HasActiveAlert = hasActiveAlert,
                PreviousNames = previousNameHelper.GetFullPreviousNames(contactDetail.PreviousNames, contactDetail.Contact)
                    .Select(name => StringHelper.JoinNonEmpty(' ', name.FirstName, name.MiddleName, name.LastName))
                    .ToArray(),
                IsActive = contact.StatusCode == Contact_StatusCode.Alive
            };
        }
    }

    public record PersonInfo
    {
        public required string Name { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required string? Email { get; init; }
        public required string? MobileNumber { get; init; }
        public required Gender? Gender { get; init; }
        public required bool HasActiveAlert { get; init; }
        public required string[] PreviousNames { get; init; }
        public required bool IsActive { get; init; }
    }

    public record PersonProfessionalStatusInfo
    {
        public required InductionStatus InductionStatus { get; init; }
        public required DateOnly? QtsDate { get; init; }
        public required QtlsStatus QtlsStatus { get; init; }
        public required DateOnly? EytsDate { get; init; }
        public required bool HasEyps { get; init; }
        public required DateOnly? PqtsDate { get; init; }
    }
}

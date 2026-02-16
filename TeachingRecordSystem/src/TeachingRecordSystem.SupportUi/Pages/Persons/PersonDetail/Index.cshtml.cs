using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

[AllowDeactivatedPerson]
public class IndexModel(TrsDbContext dbContext, IAuthorizationService authorizationService) : PageModel
{
    private static readonly InductionStatus[] _invalidInductionStatusesForMerge = [InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed];

    [FromRoute]
    public Guid PersonId { get; set; }

    public PersonInfo? Person { get; set; }

    public PersonProfessionalStatusInfo? PersonProfessionalStatus { get; set; }

    public bool HasOpenAlert { get; set; }

    public bool CanChangeDetails { get; set; }
    public bool CanMerge { get; set; }
    public bool CanSetStatus { get; set; }

    public ConnectedOneLoginUserInfo[]? ConnectedOneLoginUsers { get; set; }

    public async Task OnGetAsync()
    {
        HasOpenAlert = await dbContext.Alerts.AnyAsync(a => a.PersonId == PersonId && a.IsOpen);

        var person = await dbContext.Persons
            .IgnoreQueryFilters()
            .Include(p => p.PreviousNames).AsSplitQuery()
            .Include(p => p.Alerts).AsSplitQuery()
            .Include(p => p.OneLoginUsers).AsSplitQuery()
            .SingleAsync(p => p.PersonId == PersonId);

        Person = GetPersonInfo(person);
        PersonProfessionalStatus = GetPersonStatusInfo(person);
        ConnectedOneLoginUsers = GetConnectedOneLoginUsers(person);

        var canEditPersonData = (await authorizationService.AuthorizeAsync(User, AuthorizationPolicies.PersonDataEdit))
            .Succeeded;

        var canEditNonPersonOrAlertData = (await authorizationService.AuthorizeAsync(User, AuthorizationPolicies.NonPersonOrAlertDataEdit))
            .Succeeded;

        CanChangeDetails =
            canEditPersonData &&
            Person.IsActive;

        CanMerge =
            canEditNonPersonOrAlertData &&
            !HasOpenAlert &&
            Person!.IsActive &&
            (PersonProfessionalStatus is not PersonProfessionalStatusInfo professionalStatus ||
                !_invalidInductionStatusesForMerge.Contains(professionalStatus.InductionStatus));

        // Person cannot be reactivated if they were deactivated as part of a merge
        // where they were merged into another Person (i.e. they were the secondary
        // Person and the other Person was primary)
        var personWasDeactivatedAsPartOfAMerge = !Person.IsActive && Person.MergedWithPersonId is not null;

        CanSetStatus =
            canEditNonPersonOrAlertData &&
            !personWasDeactivatedAsPartOfAMerge;
    }

    private PersonProfessionalStatusInfo? GetPersonStatusInfo(Person person)
    {
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

    private PersonInfo GetPersonInfo(Person person)
    {
        var hasActiveAlert = person.Alerts!.Any(a => a.IsOpen);

        return new PersonInfo
        {
            Name = StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName),
            DateOfBirth = person.DateOfBirth,
            Trn = person.Trn,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Email = person.EmailAddress,
            Gender = person.Gender,
            HasActiveAlert = hasActiveAlert,
            PreviousNames = person.PreviousNames!
                .OrderByDescending(n => n.CreatedOn)
                .Select(name => StringHelper.JoinNonEmpty(' ', name.FirstName, name.MiddleName, name.LastName))
                .ToArray(),
            IsActive = person.Status == PersonStatus.Active,
            MergedWithPersonId = person.MergedWithPersonId
        };
    }

    private ConnectedOneLoginUserInfo[] GetConnectedOneLoginUsers(Person person)
    {
        return person.OneLoginUsers!
            .OrderBy(u => u.MatchedOn)
            .Select(olu => new ConnectedOneLoginUserInfo
            {
                Subject = olu.Subject,
                EmailAddress = olu.EmailAddress
            })
            .ToArray();
    }

    public record PersonInfo
    {
        public required string Name { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required string? Email { get; init; }
        public required Gender? Gender { get; init; }
        public required bool HasActiveAlert { get; init; }
        public required string[] PreviousNames { get; init; }
        public required bool IsActive { get; init; }
        public required Guid? MergedWithPersonId { get; init; }
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

    public record ConnectedOneLoginUserInfo
    {
        public required string Subject { get; init; }
        public required string? EmailAddress { get; init; }
    }
}

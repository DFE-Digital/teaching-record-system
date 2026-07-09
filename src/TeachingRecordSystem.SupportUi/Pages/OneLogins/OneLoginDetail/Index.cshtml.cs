using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail;

[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class IndexModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    public OneLoginUserInfo? OneLoginUser { get; set; }

    public ConnectedPersonInfo? ConnectedPerson { get; set; }

    public async Task OnGetAsync()
    {
        var oneLoginUser = await dbContext.OneLoginUsers
            .Include(u => u.Person)
            .IgnoreQueryFilters()
            .SingleAsync(u => u.Subject == OneLoginUserSubject);

        OneLoginUser = GetOneLoginUserInfo(oneLoginUser);

        if (oneLoginUser.PersonId.HasValue && oneLoginUser.Person is not null)
        {
            ConnectedPerson = GetConnectedPersonInfo(oneLoginUser.Person);
        }
    }

    private OneLoginUserInfo GetOneLoginUserInfo(OneLoginUser oneLoginUser)
    {
        string? verifiedName = null;
        DateOnly? verifiedDateOfBirth = null;

        if (oneLoginUser.VerifiedNames?.Length > 0)
        {
            var names = oneLoginUser.VerifiedNames[0];
            verifiedName = string.JoinNonEmpty(' ', names);
        }

        if (oneLoginUser.VerifiedDatesOfBirth?.Length > 0)
        {
            verifiedDateOfBirth = oneLoginUser.VerifiedDatesOfBirth[0];
        }

        return new OneLoginUserInfo
        {
            Subject = oneLoginUser.Subject,
            EmailAddress = oneLoginUser.EmailAddress,
            VerificationRoute = oneLoginUser.VerificationRoute,
            VerifiedName = verifiedName,
            VerifiedDateOfBirth = verifiedDateOfBirth,
            IsConnected = oneLoginUser.PersonId.HasValue
        };
    }

    private ConnectedPersonInfo GetConnectedPersonInfo(Person person)
    {
        return new ConnectedPersonInfo
        {
            PersonId = person.PersonId,
            Name = string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName),
            EmailAddress = person.EmailAddress,
            DateOfBirth = person.DateOfBirth,
            Trn = person.Trn,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            Status = person.Status
        };
    }

    public record OneLoginUserInfo
    {
        public required string Subject { get; init; }
        public required string? EmailAddress { get; init; }
        public required OneLoginUserVerificationRoute? VerificationRoute { get; init; }
        public required string? VerifiedName { get; init; }
        public required DateOnly? VerifiedDateOfBirth { get; init; }
        public required bool IsConnected { get; init; }
    }

    public record ConnectedPersonInfo
    {
        public required Guid PersonId { get; init; }
        public required string Name { get; init; }
        public required string? EmailAddress { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string Trn { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required Gender? Gender { get; init; }
        public required PersonStatus Status { get; init; }
    }
}

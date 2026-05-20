using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

[Journey(JourneyNames.ConnectPerson)]
[RequireJourneyInstance]
[TypeFilter(typeof(CheckOneLoginUserExistsFilterFactory))]
public class MatchModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ConnectPersonState>? JourneyInstance { get; set; }

    [FromRoute]
    public string OneLoginUserSubject { get; set; } = null!;

    public string? OneLoginUserEmailAddress { get; set; }
    public string[][]? OneLoginUserVerifiedNames { get; set; }
    public DateOnly[]? OneLoginUserVerifiedDatesOfBirth { get; set; }

    public string? PersonFirstName { get; set; }
    public string? PersonMiddleName { get; set; }
    public string? PersonLastName { get; set; }
    public string? PersonEmailAddress { get; set; }
    public DateOnly? PersonDateOfBirth { get; set; }
    public string? PersonTrn { get; set; }
    public string? PersonNationalInsuranceNumber { get; set; }
    public string[]? PersonConnectedOneLoginEmailAddresses { get; set; }

    public IActionResult OnPost()
    {
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.ConnectPerson.Reason(OneLoginUserSubject, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.OneLogins.OneLoginDetail.Index(OneLoginUserSubject));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.PersonId.HasValue)
        {
            context.Result = NotFound();
            return;
        }

        var oneLoginUserFeature = context.HttpContext.GetCurrentOneLoginUserFeature();
        OneLoginUserEmailAddress = oneLoginUserFeature.EmailAddress;
        OneLoginUserVerifiedNames = oneLoginUserFeature.VerifiedNames;
        OneLoginUserVerifiedDatesOfBirth = oneLoginUserFeature.VerifiedDatesOfBirth;

        var person = await dbContext.Persons
            .AsNoTracking()
            .Where(p => p.PersonId == JourneyInstance.State.PersonId)
            .SingleAsync();

        PersonFirstName = person.FirstName;
        PersonMiddleName = person.MiddleName;
        PersonLastName = person.LastName;
        PersonEmailAddress = person.EmailAddress;
        PersonDateOfBirth = person.DateOfBirth;
        PersonTrn = person.Trn;
        PersonNationalInsuranceNumber = person.NationalInsuranceNumber;

        // Get existing connected OneLogin user emails if any
        var connectedOneLoginUsers = await dbContext.OneLoginUsers
            .AsNoTracking()
            .Where(u => u.PersonId == person.PersonId && u.EmailAddress != null)
            .Select(u => u.EmailAddress!)
            .ToArrayAsync();

        PersonConnectedOneLoginEmailAddresses = connectedOneLoginUsers.Length > 0 ? connectedOneLoginUsers : null;

        await next();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin)]
public class MatchModel(
    ConnectOneLoginJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonFirstName { get; set; }
    public string? PersonMiddleName { get; set; }
    public string? PersonLastName { get; set; }
    public string? PersonEmailAddress { get; set; }
    public DateOnly? PersonDateOfBirth { get; set; }
    public string? PersonTrn { get; set; }
    public string? PersonNationalInsuranceNumber { get; set; }

    public string? OneLoginUserEmailAddress { get; set; }
    public string[][]? OneLoginUserVerifiedNames { get; set; }
    public DateOnly[]? OneLoginUserVerifiedDatesOfBirth { get; set; }

    public IReadOnlyCollection<PersonMatchedAttribute>? MatchedAttributeTypes { get; set; }

    public IActionResult OnPost()
    {
        if (Cancel)
        {
            journey.DeleteInstance();
            return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
        }

        return journey.AdvanceTo(linkGenerator.Persons.PersonDetail.ConnectOneLogin.Reason(journey.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        var personFeature = context.HttpContext.GetCurrentPersonFeature();

        PersonFirstName = personFeature.FirstName;
        PersonMiddleName = personFeature.MiddleName;
        PersonLastName = personFeature.LastName;
        PersonEmailAddress = personFeature.EmailAddress;
        PersonDateOfBirth = personFeature.DateOfBirth;
        PersonTrn = personFeature.Trn;
        PersonNationalInsuranceNumber = personFeature.NationalInsuranceNumber;

        var oneLoginUser = await dbContext.OneLoginUsers
            .AsNoTracking()
            .Where(u => u.Subject == journey.State.Subject)
            .SingleAsync();

        OneLoginUserEmailAddress = oneLoginUser.EmailAddress;
        OneLoginUserVerifiedNames = oneLoginUser.VerifiedNames;
        OneLoginUserVerifiedDatesOfBirth = oneLoginUser.VerifiedDatesOfBirth;

        // Set by the Index page, which is the only step this one can be reached from
        MatchedAttributeTypes = journey.State.MatchedAttributes!
            .Select(a => a.Key)
            .ToArray();

        await next();
    }
}

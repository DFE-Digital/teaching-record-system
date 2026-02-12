using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin)]
[ActivatesJourney, RequireJourneyInstance]
public class MatchModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ConnectOneLoginState>? JourneyInstance { get; set; }

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
    public string? OneLoginUserFirstName { get; set; }
    public string? OneLoginUserLastName { get; set; }
    public DateOnly? OneLoginUserDateOfBirth { get; set; }

    public IReadOnlyCollection<PersonMatchedAttribute>? MatchedAttributeTypes { get; set; }

    public string BackLink => linkGenerator.Persons.PersonDetail.ConnectOneLogin.Index(PersonId);

    public void OnGet()
    {
        MatchedAttributeTypes = JourneyInstance!.State.MatchedPerson?.MatchedAttributes
            .Select(a => a.Key)
            .ToArray();
    }

    public IActionResult OnPost()
    {
        return Redirect(linkGenerator.Persons.PersonDetail.ConnectOneLogin.Reason(PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
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
            .Where(u => u.Subject == JourneyInstance!.State.Subject)
            .SingleAsync();

        OneLoginUserEmailAddress = oneLoginUser.EmailAddress;

        if (oneLoginUser.VerifiedNames is not null && oneLoginUser.VerifiedNames.Length > 0)
        {
            var firstVerifiedName = oneLoginUser.VerifiedNames[0];
            OneLoginUserFirstName = firstVerifiedName.First();
            OneLoginUserLastName = firstVerifiedName.LastOrDefault();
        }

        if (oneLoginUser.VerifiedDatesOfBirth is not null && oneLoginUser.VerifiedDatesOfBirth.Length > 0)
        {
            OneLoginUserDateOfBirth = oneLoginUser.VerifiedDatesOfBirth[0];
        }

        await next();
    }
}

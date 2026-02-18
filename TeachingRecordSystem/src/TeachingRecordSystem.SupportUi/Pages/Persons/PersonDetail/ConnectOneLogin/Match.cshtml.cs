using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

[Journey(JourneyNames.ConnectOneLogin)]
[RequireJourneyInstance]
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
    public string[][]? OneLoginUserVerifiedNames { get; set; }
    public DateOnly[]? OneLoginUserVerifiedDatesOfBirth { get; set; }

    public IReadOnlyCollection<PersonMatchedAttribute>? MatchedAttributeTypes { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        return Redirect(linkGenerator.Persons.PersonDetail.ConnectOneLogin.Reason(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Index(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance!.State.MatchedPerson is null)
        {
            context.Result = NotFound();
            return;
        }

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
        OneLoginUserVerifiedNames = oneLoginUser.VerifiedNames;
        OneLoginUserVerifiedDatesOfBirth = oneLoginUser.VerifiedDatesOfBirth;

        MatchedAttributeTypes = JourneyInstance.State.MatchedPerson.MatchedAttributes
            .Select(a => a.Key)
            .ToArray();

        await next();
    }
}

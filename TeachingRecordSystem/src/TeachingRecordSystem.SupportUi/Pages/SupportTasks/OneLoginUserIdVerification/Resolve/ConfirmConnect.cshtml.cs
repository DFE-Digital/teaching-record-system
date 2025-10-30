using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class ConfirmConnect(
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    OneLoginService oneLoginService,
    IClock clock,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    public Guid MatchedPersonId { get; set; }

    public string? OneLoginUserEmailAddress { get; set; }

    public string? MatchedPersonName { get; set; }

    public string? MatchedPersonEmailAddress { get; set; }

    public IReadOnlyCollection<string>? MatchedPersonPreviousNames { get; set; }

    public string? MatchedPersonTrn { get; set; }

    public DateOnly MatchedPersonDateOfBirth { get; set; }

    public string? MatchedPersonNationalInsuranceNumber { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(bool cancel)
    {
        if (cancel)
        {
            await JourneyInstance.DeleteAsync();

            return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
        }

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, clock.UtcNow, User.GetUserId());

        var data = _supportTask!.GetData<OneLoginUserIdVerificationData>();

        var matchedAttributes = JourneyInstance.State.MatchedPersons
            .Single(m => m.PersonId == MatchedPersonId)
            .MatchedAttributes
            .Select(a => KeyValuePair.Create(a, a switch
            {
                PersonMatchedAttribute.FirstName => data.StatedFirstName,
                PersonMatchedAttribute.LastName => data.StatedLastName,
                PersonMatchedAttribute.FullName => $"{data.StatedFirstName} {data.StatedLastName}",
                PersonMatchedAttribute.DateOfBirth => data.StatedDateOfBirth.ToString("yyyy-MM-dd"),
                PersonMatchedAttribute.NationalInsuranceNumber => data.StatedNationalInsuranceNumber,
                PersonMatchedAttribute.Trn => data.StatedTrn,
                PersonMatchedAttribute.EmailAddress => _supportTask.OneLoginUserSubject!,
                _ => throw new NotSupportedException($"Unknown {nameof(PersonMatchedAttribute)}: '{a}'.")
            }));

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = _supportTask.OneLoginUserSubject!,
                VerificationRoute = OneLoginUserVerificationRoute.Support,
                VerifiedDatesOfBirth = [data.StatedDateOfBirth],
                VerifiedNames = [[data.StatedFirstName, data.StatedLastName]]
            },
            processContext);

        await oneLoginService.SetUserMatchedAsync(
            new SetUserMatchedOptions
            {
                OneLoginUserSubject = _supportTask.OneLoginUserSubject!,
                MatchedPersonId = MatchedPersonId,
                MatchRoute = OneLoginUserMatchRoute.Support,
                MatchedAttributes = matchedAttributes!
            },
            processContext);

        await oneLoginService.EnqueueRecordMatchedEmailAsync(_supportTask.OneLoginUser!.EmailAddress!, MatchedPersonName!, processContext);

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<OneLoginUserIdVerificationData>
            {
                SupportTaskReference = SupportTaskReference,
                UpdateData = data => data with
                {
                    Verified = true,
                    PersonId = MatchedPersonId,
                    Outcome = OneLoginUserIdVerificationOutcome.VerifiedAndConnected
                },
                Status = SupportTaskStatus.Closed
            },
            processContext);

        await JourneyInstance.DeleteAsync();

        TempData.SetFlashSuccess(
            $"GOV.UK One Login account connected to {MatchedPersonName}’s record",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(MatchedPersonId)));

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersonId is null)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Matches(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        OneLoginUserEmailAddress = _supportTask.OneLoginUser!.EmailAddress;

        var matchedPerson = await dbContext.Persons
            .Include(p => p.PreviousNames)
            .SingleAsync(p => p.PersonId == JourneyInstance.State.MatchedPersonId);

        MatchedPersonId = matchedPerson.PersonId;
        MatchedPersonName = $"{matchedPerson.FirstName} {matchedPerson.MiddleName} {matchedPerson.LastName}";
        MatchedPersonEmailAddress = matchedPerson.EmailAddress;
        MatchedPersonPreviousNames = matchedPerson.PreviousNames!
            .OrderBy(n => n.CreatedOn)
            .Select(n => $"{n.FirstName} {n.MiddleName} {n.LastName}")
            .ToList();
        MatchedPersonTrn = matchedPerson.Trn;
        MatchedPersonDateOfBirth = matchedPerson.DateOfBirth!.Value;
        MatchedPersonNationalInsuranceNumber = matchedPerson.NationalInsuranceNumber;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

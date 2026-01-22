using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserRecordMatching;
using TeachingRecordSystem.SupportUi;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching), RequireJourneyInstance]
public class ConfirmConnect(
    OneLoginUserMatchingSupportTaskService supportTaskService,
    TrsDbContext dbContext,
    IClock clock,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    public JourneyInstance<ResolveOneLoginUserMatchingState> JourneyInstance { get; set; } = null!;

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

            if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
            {
                return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
            }

            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
        }

        var matchedPerson = JourneyInstance.State.MatchedPersons
            .Single(m => m.PersonId == MatchedPersonId);

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, clock.UtcNow, User.GetUserId());

            await supportTaskService.ResolveSupportTaskAsync(
                new VerifiedAndConnectedOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    MatchedPersonId = MatchedPersonId,
                    MatchedAttributes = matchedPerson.MatchedAttributes
                },
                processContext);
        }
        else
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, clock.UtcNow, User.GetUserId());

            await supportTaskService.ResolveSupportTaskAsync(
                new ConnectedOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    MatchedPersonId = MatchedPersonId,
                    MatchedAttributes = matchedPerson.MatchedAttributes
                },
                processContext);
        }

        await JourneyInstance.DeleteAsync();

        TempData.SetFlashSuccess(
            $"GOV.UK One Login connected to {MatchedPersonName}’s record",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(MatchedPersonId)));

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification());
        }

        return Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersonId is null)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.Matches(SupportTaskReference, JourneyInstance.InstanceId));
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class ConfirmConnect(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    OneLoginUserMatchingSupportTaskService supportTaskService,
    TrsDbContext dbContext,
    TimeProvider timeProvider,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private SupportTask? _supportTask;

    [BindProperty]
    public bool Cancel { get; set; }

    public string? BackLink { get; set; }

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

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            journey.DeleteInstance();

            return Redirect(GetListPageUrl());
        }

        var matchedPerson = journey.State.MatchedPersons
            .Single(m => m.PersonId == MatchedPersonId);

        if (_supportTask!.SupportTaskType == SupportTaskType.OneLoginUserIdVerification)
        {
            var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            await supportTaskService.ResolveVerificationSupportTaskAsync(
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
            var processContext = new ProcessContext(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, timeProvider.UtcNow, User.GetUserId());

            await supportTaskService.ResolveRecordMatchingSupportTaskAsync(
                new ConnectedOutcomeOptions
                {
                    SupportTask = _supportTask!,
                    MatchedPersonId = MatchedPersonId,
                    MatchedAttributes = matchedPerson.MatchedAttributes
                },
                processContext);
        }

        journey.DeleteInstance();

        TempData.SetFlashNotificationBanner(
            $"GOV.UK One Login connected to {MatchedPersonName}’s record",
            $"We’ve sent {MatchedPersonName} an email confirming their GOV.UK One Login has been connected to their teaching record.");

        return Redirect(GetListPageUrl());
    }

    private string GetListPageUrl() =>
        _supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            linkGenerator.SupportTasks.OneLoginUserMatching.IdVerification() :
            linkGenerator.SupportTasks.OneLoginUserMatching.RecordMatching();

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = context.HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        BackLink = journey.GetBackLink();

        OneLoginUserEmailAddress = _supportTask.OneLoginUser!.EmailAddress;

        var matchedPerson = await dbContext.Persons
            .Include(p => p.PreviousNames)
            .SingleAsync(p => p.PersonId == journey.State.MatchedPersonId);

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

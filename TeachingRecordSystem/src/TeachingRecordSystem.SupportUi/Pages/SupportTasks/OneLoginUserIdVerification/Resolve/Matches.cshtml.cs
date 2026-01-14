using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class Matches(
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    IClock clock,
    SupportUiLinkGenerator linkGenerator) :
    PageModel
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    private readonly InlineValidator<Matches> _validator = new()
    {
        v => v.RuleFor(m => m.MatchedPersonId)
            .NotNull().WithMessage("Select what you want to do with this GOV.UK One Login")
    };

    private SupportTask? _supportTask;

    public JourneyInstance<ResolveOneLoginUserIdVerificationState> JourneyInstance { get; set; } = null!;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    public Guid? MatchedPersonId { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }

    public IReadOnlyCollection<SuggestedMatchViewModel>? SuggestedMatches { get; set; }

    public void OnGet()
    {
        JourneyInstance.State.ApplySavedModelStateValues(nameof(Matches), this.ModelState);
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            return await HandleCancelAsync();
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        await JourneyInstance.UpdateStateAsync(state =>
        {
            state.MatchedPersonId = MatchedPersonId;
            state.ClearSavedModelStateValues(nameof(Matches));
        });

        return MatchedPersonId != ResolveOneLoginUserIdVerificationState.NotMatchedPersonIdSentinel ?
            Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.ConfirmConnect(SupportTaskReference, JourneyInstance.InstanceId)) :
            Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.NotConnecting(SupportTaskReference, JourneyInstance.InstanceId));
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(Matches),
            JourneyInstance.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processContext = new ProcessContext(ProcessType.OneLoginUserIdVerificationSupportTaskSaving, clock.UtcNow, User.GetUserId());

        await supportTaskService.UpdateSupportTaskAsync(
            new()
            {
                SupportTask = _supportTask!.SupportTaskReference,
                Status = SupportTaskStatus.InProgress,
                SavedJourneyState = Option.Some(savedJourneyState)!
            },
            processContext);

        await JourneyInstance.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    private async Task<IActionResult> HandleCancelAsync()
    {
        await JourneyInstance.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance.State.Verified is not true)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.Index(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        if (JourneyInstance.State.MatchedPersons.Count == 0)
        {
            context.Result = Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Resolve.NoMatches(SupportTaskReference, JourneyInstance.InstanceId));
            return;
        }

        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var oneLoginUser = _supportTask.OneLoginUser!;
        var data = _supportTask.GetData<OneLoginUserIdVerificationData>();

        Name = $"{data.StatedFirstName} {data.StatedLastName}";
        DateOfBirth = data.StatedDateOfBirth;
        NationalInsuranceNumber = data.StatedNationalInsuranceNumber;
        Trn = data.StatedTrn;
        EmailAddress = oneLoginUser.EmailAddress;

        var matchedPersonIds = JourneyInstance.State.MatchedPersons.Select(m => m.PersonId).ToArray();
        SuggestedMatches = (await dbContext.Persons
            .Include(p => p.PreviousNames)
            .Where(p => matchedPersonIds.Contains(p.PersonId))
            .Select(p => new
            {
                p.PersonId,
                p.Trn,
                p.EmailAddress,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.NationalInsuranceNumber,
                p.PreviousNames
            })
            .ToArrayAsync())
            .OrderBy(p => Array.IndexOf(matchedPersonIds, p.PersonId))  // Ensure we maintain the order of matches
            .Select((match, idx) => new SuggestedMatchViewModel
            {
                Identifier = (char)('A' + idx),
                PersonId = match.PersonId,
                Trn = match.Trn,
                EmailAddress = match.EmailAddress,
                FirstName = match.FirstName,
                LastName = match.LastName,
                DateOfBirth = match.DateOfBirth,
                NationalInsuranceNumber = match.NationalInsuranceNumber,
                PreviousNames = match.PreviousNames!
                    .OrderBy(n => n.CreatedOn)
                    .Select(n => $"{n.FirstName} {n.MiddleName} {n.LastName}")
                    .ToArray(),
                MatchedAttributeTypes = JourneyInstance.State.MatchedPersons.Single(m => m.PersonId == match.PersonId)
                    .MatchedAttributes
                    .Select(kvp => kvp.Key)
                    .ToArray()
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

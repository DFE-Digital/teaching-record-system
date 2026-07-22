using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserMatching)]
public class Matches(
    ResolveOneLoginUserMatchingJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
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

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    public Guid? MatchedPersonId { get; set; }

    public string? BackLink { get; set; }

    public string? Name { get; set; }
    public string? EmailAddress { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }

    public IReadOnlyCollection<SuggestedMatchViewModel>? SuggestedMatches { get; set; }

    public void OnGet()
    {
        journey.State.ApplySavedModelStateValues(nameof(Matches), ModelState);
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            journey.DeleteInstance();

            return Redirect(journey.GetListPageUrl());
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
        }

        await _validator.ValidateAndThrowAsync(this);

        var nextStepUrl = MatchedPersonId != ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel ?
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.ConfirmConnect(journey.InstanceId) :
            linkGenerator.SupportTasks.OneLoginUserMatching.Resolve.NotConnecting(journey.InstanceId);

        return journey.AdvanceTo(
            nextStepUrl,
            state =>
            {
                state.MatchedPersonId = MatchedPersonId;
                state.ClearSavedModelStateValues(nameof(Matches));
            });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(Matches),
            journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processType = _supportTask!.SupportTaskType is SupportTaskType.OneLoginUserIdVerification ?
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving :
            ProcessType.OneLoginUserRecordMatchingSupportTaskSaving;

        var processContext = new ProcessContext(processType, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = _supportTask.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        journey.DeleteInstance();

        return Redirect(journey.GetListPageUrl());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        BackLink = journey.GetBackLink() ?? journey.GetListPageUrl();

        var oneLoginUser = _supportTask.OneLoginUser!;
        var data = _supportTask.GetData<IOneLoginUserMatchingData>();

        // For the time being only display first verified name and dob if there are multiples (but still match on both)
        var firstVerifiedOrStatedName = data.VerifiedOrStatedNames!.First();
        Name = $"{firstVerifiedOrStatedName.First()} {firstVerifiedOrStatedName.LastOrDefault()}";
        DateOfBirth = data.VerifiedOrStatedDatesOfBirth!.First();
        NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(data.StatedNationalInsuranceNumber);
        Trn = TrnHelper.NormalizeTrn(data.StatedTrn);
        EmailAddress = oneLoginUser.EmailAddress;

        var matchedPersonIds = journey.State.MatchedPersons.Select(m => m.PersonId).ToArray();
        SuggestedMatches = (await dbContext.Persons
            .Include(p => p.PreviousNames)
            .Where(p => matchedPersonIds.Contains(p.PersonId))
            .Select(p => new
            {
                p.PersonId,
                p.Trn,
                p.EmailAddress,
                p.FirstName,
                p.MiddleName,
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
                MiddleName = match.MiddleName,
                LastName = match.LastName,
                DateOfBirth = match.DateOfBirth,
                NationalInsuranceNumber = match.NationalInsuranceNumber,
                PreviousNames = match.PreviousNames!
                    .OrderBy(n => n.CreatedOn)
                    .Select(n => $"{n.FirstName} {n.MiddleName} {n.LastName}")
                    .ToArray(),
                MatchedAttributeTypes = journey.State.MatchedPersons.Single(m => m.PersonId == match.PersonId)
                    .MatchedAttributes
                    .Select(kvp => kvp.Key)
                    .ToArray()
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

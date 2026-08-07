using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate)]
public class MatchesModel(
    ResolveTeacherPensionsPotentialDuplicateJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportTaskService supportTaskService,
    TimeProvider timeProvider,
    IFeatureProvider featureProvider,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceController) : ResolveTeacherPensionsPotentialDuplicatePageModel(journey, dbContext)
{
    public static class Actions
    {
        public const string SaveAndComeBackLater = nameof(SaveAndComeBackLater);
        public const string Cancel = nameof(Cancel);
    }

    private readonly InlineValidator<MatchesModel> _validator = new()
    {
        v => v.RuleFor(m => m.PersonId)
            .NotNull().WithMessage("Select a record")
    };

    public TrnRequestMetadata? RequestData { get; set; }

    public SupportTask? SupportTask { get; set; }

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    public string Name => string.JoinNonEmpty(' ', RequestData?.FirstName, RequestData?.MiddleName, RequestData?.LastName);

    public IReadOnlyCollection<(PotentialDuplicate PotentialDuplicate, bool HasNameMismatch)> PotentialDuplicatesWithNameMatchingInfo { get; set; } = Array.Empty<(PotentialDuplicate, bool)>();

    [BindProperty]
    public Guid? PersonId { get; set; }

    public string? Trn { get; set; }

    public string[]? OneLoginEmails { get; set; } = Array.Empty<string>();

    public void OnGet()
    {
        Journey.State.ApplySavedModelStateValues(nameof(MatchesModel), ModelState);
        PersonId = Journey.State.PersonId;
        OneLoginEmails = DbContext.OneLoginUsers.Where(x => x.PersonId == SupportTask!.PersonId).Select(x => x.EmailAddress!)
            .ToArray();
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        if (action is Actions.Cancel)
        {
            return await CancelAsync();
        }

        if (action is Actions.SaveAndComeBackLater)
        {
            return await HandleSaveAndReturnAsync();
        }

        // Verify the submitted ID is legit
        if (PersonId is Guid personId && PersonId != Guid.Empty &&
            (!PotentialDuplicates!.Any(d => d.PersonId == personId)))
        {
            return BadRequest();
        }

        await this.ThrowIfInvalidAsync(_validator);

        var nextStepUrl = PersonId == ResolveTeacherPensionsPotentialDuplicateState.KeepRecordSeparatePersonIdSentinel ?
            linkGenerator.SupportTasks.TeacherPensions.Resolve.KeepRecordSeparate(Journey.InstanceId) :
            linkGenerator.SupportTasks.TeacherPensions.Resolve.Merge(Journey.InstanceId);

        return Journey.AdvanceTo(nextStepUrl, state =>
        {
            var oldPersonId = state.PersonId;
            state.PersonId = PersonId;
            state.ClearSavedModelStateValues(nameof(MatchesModel));

            if (oldPersonId != PersonId)
            {
                state.FirstNameSource = null;
                state.MiddleNameSource = null;
                state.LastNameSource = null;
                state.DateOfBirthSource = null;
                state.NationalInsuranceNumberSource = null;
                state.GenderSource = null;
                state.PersonAttributeSourcesSet = false;
                state.TeachersPensionPersonId = SupportTask!.PersonId!;
            }
        });
    }

    private async Task<IActionResult> HandleSaveAndReturnAsync()
    {
        var savedJourneyState = this.CreateSavedJourneyState(
            nameof(MatchesModel),
            Journey.State,
            excludeKeys: ["Action", nameof(SupportTaskReference)]);

        var processContext = new ProcessContext(ProcessType.TeacherPensionsSupportTaskSaving, timeProvider.UtcNow, User.GetUserId());

        await supportTaskService.SaveProgressAsync(
            new()
            {
                SupportTaskReference = SupportTask!.SupportTaskReference,
                SavedJourneyState = savedJourneyState
            },
            processContext);

        Journey.DeleteInstance();

        if (featureProvider.IsEnabled("SupportTaskDashboard"))
        {
            return Redirect(linkGenerator.SupportTasks.SupportTaskDetail.Index(SupportTask.SupportTaskReference));
        }

        return Redirect(Journey.State.CompletionUrl);
    }

    private async Task<IActionResult> CancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(Journey.State.Evidence.UploadedEvidenceFile);
        Journey.DeleteInstance();

        return Redirect(Journey.State.CompletionUrl);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        SupportTask = GetSupportTask();
        RequestData = SupportTask!.TrnRequestMetadata!;

        BackLink = Journey.GetBackLink() ?? Journey.State.CompletionUrl;

        var person = await DbContext.Persons.Include(x => x.OneLoginUsers).SingleOrDefaultAsync(x => x.PersonId == SupportTask!.PersonId);
        if (person != null)
        {
            Trn = person.Trn;
        }

        var matchedAttributesLookup = Journey.State.MatchedPersons.ToDictionary(
                mp => mp.PersonId,
                mp => mp.MatchedAttributes);
        var matchedPersonIds = Journey.State.MatchedPersons.Select(p => p.PersonId).ToArray();

        PotentialDuplicates = (await DbContext.Persons.Include(x => x.OneLoginUsers)
            .Where(p => matchedPersonIds.Contains(p.PersonId))
            .Select(p => new PotentialDuplicate
            {
                Identifier = 'X', // We'll fix this below, can't do it over an IQueryable
                MatchedAttributes = Array.Empty<PersonMatchedAttribute>(),  // ditto
                PersonId = p.PersonId,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                EmailAddress = p.EmailAddress,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                Gender = p.Gender,
                Trn = p.Trn,
                HasQts = p.QtsDate != null,
                HasEyts = p.EytsDate != null,
                PreviousNames = p.PreviousNames!
                    .OrderBy(n => n.CreatedOn)
                    .Select(n => string.JoinNonEmpty(' ', n.FirstName, n.MiddleName, n.LastName))
                    .ToArray(),
                HasActiveAlerts = p.Alerts!.Any(a => a.IsOpen)
            })
            .ToArrayAsync())
            // matchedPersonIds is ordered by best match first; ensure we maintain that order
            .OrderBy(p => Array.IndexOf(matchedPersonIds, p.PersonId))
            .Select((r, i) => r with
            {
                Identifier = (char)('A' + i),
                MatchedAttributes = matchedAttributesLookup[r.PersonId]
            })
            .ToArray();

        // highlight name mismatches taking into account whether each name part is present in the request data and the match
        PotentialDuplicatesWithNameMatchingInfo = PotentialDuplicates!
            .Select(pd => (pd, HasNameMismatch: pd.HasAnyNamePartMismatch(RequestData!.FirstName, RequestData.MiddleName, RequestData.LastName)))
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;

[Journey(JourneyNames.ResolveTrnRequest)]
public class Matches(
    ResolveTrnRequestJourneyCoordinator journey,
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator) : ResolveTrnRequestPageModel(journey, dbContext)
{
    private readonly InlineValidator<Matches> _validator = new()
    {
        v => v.RuleFor(m => m.PersonId)
            .NotNull().WithMessage("Select a record")
    };

    [BindProperty]
    public bool Cancel { get; set; }

    public TrnRequestMetadata? RequestData { get; set; }

    public MatchPersonsResultOutcome MatchOutcome { get; set; }

    public string SourceApplicationUserName => RequestData!.ApplicationUser!.Name;

    public string Name => string.JoinNonEmpty(' ', RequestData!.FirstName, RequestData!.MiddleName, RequestData!.LastName);

    public IReadOnlyCollection<(PotentialDuplicate PotentialDuplicate, bool HasNameMismatch)> PotentialDuplicatesWithNameMatchingInfo { get; set; } = Array.Empty<(PotentialDuplicate, bool)>();

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    [BindProperty]
    public Guid? PersonId { get; set; }

    public void OnGet()
    {
        PersonId = Journey.State.PersonId;
    }

    public IActionResult OnPost()
    {
        if (Cancel)
        {
            Journey.DeleteInstance();

            return Redirect(linkGenerator.SupportTasks.TrnRequests.Index());
        }

        // Verify the submitted ID is legit
        if (PersonId is Guid personId &&
            (!PotentialDuplicates!.Any(d => d.PersonId == personId) && personId != ResolveTrnRequestState.CreateNewRecordPersonIdSentinel))
        {
            return BadRequest();
        }

        _validator.ValidateAndThrow(this);

        var nextStepUrl = PersonId == ResolveTrnRequestState.CreateNewRecordPersonIdSentinel ?
            linkGenerator.SupportTasks.TrnRequests.Resolve.CheckAnswers(Journey.InstanceId) :
            linkGenerator.SupportTasks.TrnRequests.Resolve.Merge(Journey.InstanceId);

        return Journey.AdvanceTo(nextStepUrl, state =>
        {
            var oldPersonId = state.PersonId;
            state.PersonId = PersonId;

            if (oldPersonId != PersonId)
            {
                state.FirstNameSource = null;
                state.MiddleNameSource = null;
                state.LastNameSource = null;
                state.DateOfBirthSource = null;
                state.EmailAddressSource = null;
                state.NationalInsuranceNumberSource = null;
                state.GenderSource = null;
                state.PersonAttributeSourcesSet = false;
            }
        });
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RequestData = GetRequestData();

        BackLink = Journey.GetBackLink() ?? linkGenerator.SupportTasks.TrnRequests.Index();

        var matchedAttributesLookup = Journey.State.MatchedPersons.ToDictionary(
                mp => mp.PersonId,
                mp => mp.MatchedAttributes);
        var matchedPersonIds = Journey.State.MatchedPersons.Select(p => p.PersonId).ToArray();
        MatchOutcome = Journey.State.MatchOutcome;

        PotentialDuplicates = (await DbContext.Persons
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance]
public class MatchesModel(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : ResolveNpqTrnRequestPageModel(dbContext)
{
    private readonly InlineValidator<MatchesModel> _validator = new()
    {
        v => v.RuleFor(m => m.PersonId)
            .NotNull().WithMessage("Select a record")
    };

    public TrnRequestMetadata? RequestData { get; set; }

    public MatchPersonsResultOutcome MatchOutcome { get; set; }

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    public UploadedEvidenceFile? NpqEvidenceFile { get; set; }

    public string SourceApplicationUserName => RequestData!.ApplicationUser!.Name;

    public string Name => StringHelper.JoinNonEmpty(' ', RequestData?.FirstName, RequestData?.MiddleName, RequestData?.LastName);

    [BindProperty]
    public Guid? PersonId { get; set; }

    public void OnGet()
    {
        PersonId = JourneyInstance!.State.PersonId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate the submitted ID
        if (PersonId is Guid personId &&
            !PotentialDuplicates!.Any(d => d.PersonId == personId) && personId != ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel)
        {
            return BadRequest();
        }

        _validator.ValidateAndThrow(this);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            var oldPersonId = state.PersonId;
            state.PersonId = PersonId;

            if (oldPersonId != PersonId)
            {
                state.DateOfBirthSource = null;
                state.EmailAddressSource = null;
                state.NationalInsuranceNumberSource = null;
                state.GenderSource = null;
                state.PersonAttributeSourcesSet = false;
            }
        });

        return Redirect(
            PersonId == ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel ?
                linkGenerator.SupportTasks.NpqTrnRequests.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.SupportTasks.NpqTrnRequests.Resolve.Merge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RequestData = GetRequestData();

        var matchedAttributesLookup = JourneyInstance!.State.MatchedPersons.ToDictionary(
                mp => mp.PersonId,
                mp => mp.MatchedAttributes);
        var matchedPersonIds = JourneyInstance!.State.MatchedPersons.Select(p => p.PersonId).ToArray();
        MatchOutcome = JourneyInstance.State.MatchOutcome;

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
                Trn = p.Trn,
                Gender = p.Gender,
                HasQts = p.QtsDate != null,
                HasEyts = p.EytsDate != null,
                PreviousNames = p.PreviousNames!
                    .OrderBy(n => n.CreatedOn)
                    .Select(n => StringHelper.JoinNonEmpty(' ', n.FirstName, n.MiddleName, n.LastName))
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

        NpqEvidenceFile = (RequestData?.NpqEvidenceFileId, RequestData?.NpqEvidenceFileName) is (Guid fileId, string fileName)
            ? new(fileId, fileName) : null;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

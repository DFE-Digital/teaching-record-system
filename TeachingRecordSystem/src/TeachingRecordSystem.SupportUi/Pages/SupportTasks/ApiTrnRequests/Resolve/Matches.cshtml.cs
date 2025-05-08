using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance, ActivatesJourney]
public class Matches(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveApiTrnRequestPageModel(dbContext)
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    public TrnRequestMetadata? RequestData { get; set; }

    public string SourceApplicationUserName => RequestData!.ApplicationUser.Name;

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a record")]
    public Guid? PersonId { get; set; }

    public void OnGet()
    {
        PersonId = JourneyInstance!.State.PersonId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Verify the submitted ID is legit
        if (PersonId is Guid personId &&
            (!PotentialDuplicates!.Any(d => d.PersonId == personId) && personId != ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel))
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
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
                state.PersonAttributeSourcesSet = false;
            }
        });

        return Redirect(
            PersonId == ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel ?
                linkGenerator.ApiTrnRequestCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.ApiTrnRequestMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.ApiTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RequestData = GetRequestData();

        if (RequestData.PotentialDuplicate != true ||
            RequestData.Matches is not { MatchedRecords.Count: >= 1 })
        {
            context.Result = BadRequest();
            return;
        }

        var matchedPersonIds = RequestData.Matches.MatchedRecords.Select(m => m.PersonId).ToArray();

        PotentialDuplicates = (await DbContext.Persons
                .Where(p => matchedPersonIds.Contains(p.PersonId) && p.DqtState == (int)ContactState.Active)
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
                    Trn = p.Trn!,
                    HasQts = p.QtsDate != null,
                    HasEyts = p.EytsDate != null,
                    HasActiveAlerts = p.Alerts.Any(a => a.IsOpen)
                })
                .ToArrayAsync())
            // matchedPersonIds is ordered by best match first; ensure we maintain that order
            .OrderBy(p => Array.IndexOf(matchedPersonIds, p.PersonId))
            .Select((r, i) => r with
            {
                Identifier = (char)('A' + i),
                MatchedAttributes = GetPersonAttributeMatches(
                    r.FirstName,
                    r.MiddleName,
                    r.LastName,
                    r.DateOfBirth,
                    r.EmailAddress,
                    r.NationalInsuranceNumber)
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public record PotentialDuplicate
    {
        public required char Identifier { get; init; }
        public required Guid PersonId { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required DateOnly? DateOfBirth { get; init; }
        public required string? EmailAddress { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required string Trn { get; init; }
        // TODO Gender
        public required bool HasQts { get; init; }
        public required bool HasEyts { get; init; }
        public required bool HasActiveAlerts { get; init; }
        public required IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes { get; init; }
    }
}

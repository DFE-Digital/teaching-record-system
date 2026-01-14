using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance]
public class Matches(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : ResolveApiTrnRequestPageModel(dbContext)
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public TrnRequestMetadata? RequestData { get; set; }

    public MatchPersonsResultOutcome MatchOutcome { get; set; }

    public string SourceApplicationUserName => RequestData!.ApplicationUser!.Name;

    public string Name => StringHelper.JoinNonEmpty(' ', RequestData!.FirstName, RequestData!.MiddleName, RequestData!.LastName);

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
                state.GenderSource = null;
                state.PersonAttributeSourcesSet = false;
            }
        });

        return Redirect(
            PersonId == ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel ?
                linkGenerator.SupportTasks.ApiTrnRequests.Resolve.CheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.SupportTasks.ApiTrnRequests.Resolve.Merge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.ApiTrnRequests.Index());
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
                Gender = p.Gender,
                Trn = p.Trn,
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

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

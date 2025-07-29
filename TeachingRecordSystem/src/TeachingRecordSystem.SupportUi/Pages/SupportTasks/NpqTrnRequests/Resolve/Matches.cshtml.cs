using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.Matches;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;

[Journey(JourneyNames.ResolveNpqTrnRequest), RequireJourneyInstance, ActivatesJourney]
public class MatchesModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveNpqTrnRequestPageModel(dbContext)
{
    public TrnRequestMetadata? RequestData { get; set; }

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    public string SourceApplicationUserName => RequestData!.ApplicationUser!.Name;

    [BindProperty]
    [Required(ErrorMessage = "Select a record")]
    public Guid? PersonId { get; set; }

    public void OnGet()
    {
        PersonId = JourneyInstance!.State.PersonId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validate the submitted ID
        if (PersonId is Guid personId &&
            !PotentialDuplicates!.Any(d => d.PersonId == personId) && personId != ResolveApiTrnRequestState.CreateNewRecordPersonIdSentinel)
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
                state.DateOfBirthSource = null;
                state.EmailAddressSource = null;
                state.NationalInsuranceNumberSource = null;
                state.PersonAttributeSourcesSet = false;
            }
        });

        return Redirect(
            PersonId == ResolveNpqTrnRequestState.CreateNewRecordPersonIdSentinel ?
                linkGenerator.NpqTrnRequestCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.NpqTrnRequestMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks());
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
                Trn = p.Trn!,
                Gender = p.Gender,
                HasQts = p.QtsDate != null,
                HasEyts = p.EytsDate != null,
                HasActiveAlerts = p.Alerts!.Any(a => a.IsOpen)
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
}

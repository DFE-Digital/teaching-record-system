using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance]
public class Matches(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : ResolveTeacherPensionsPotentialDuplicatePageModel(dbContext)
{
    public TrnRequestMetadata? RequestData { get; set; }

    public SupportTask? SupportTask { get; set; }

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a record")]
    public Guid? PersonId { get; set; }

    public string? Trn { get; set; }

    public void OnGet()
    {
        PersonId = JourneyInstance!.State.PersonId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Verify the submitted ID is legit
        if (PersonId is Guid personId && PersonId != Guid.Empty &&
            (!PotentialDuplicates!.Any(d => d.PersonId == personId)))
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
                state.NationalInsuranceNumberSource = null;
                state.GenderSource = null;
                state.PersonAttributeSourcesSet = false;
                state.TeachersPensionPersonId = SupportTask!.PersonId!;
            }
        });

        return Redirect(
            PersonId == Guid.Empty ?
                linkGenerator.TeacherPensionsKeepRecordSeparate(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.TeacherPensionsMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.TeacherPensions());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        SupportTask = GetSupportTask();
        RequestData = SupportTask!.TrnRequestMetadata!;

        var person = await DbContext.Persons.SingleAsync(x => x.PersonId == SupportTask!.PersonId);
        if (person != null)
        {
            Trn = person.Trn!;
        }

        var matchedPersonIds = JourneyInstance!.State.MatchedPersonIds.ToArray();

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
                Trn = p.Trn!,
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
                    r.NationalInsuranceNumber,
                    r.Gender)
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

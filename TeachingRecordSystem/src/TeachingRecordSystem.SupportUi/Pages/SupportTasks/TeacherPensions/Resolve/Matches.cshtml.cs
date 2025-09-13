using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

[Journey(JourneyNames.ResolveTpsPotentialDuplicate), RequireJourneyInstance, ActivatesJourney]
public class Matches(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    [FromRoute]
    public string? SupportTaskReference { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public TrnRequestMetadata? RequestData { get; set; }

    public PotentialDuplicate[]? PotentialDuplicates { get; set; }

    public JourneyInstance<ResolveTeacherPensionsPotentialDuplicateState>? JourneyInstance { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a record")]
    public Guid? PersonId { get; set; }

    protected TrnRequestMetadata GetRequestData()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        return supportTask.TrnRequestMetadata!;
    }

    public void OnGet()
    {
        PersonId = JourneyInstance!.State.PersonId;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Verify the submitted ID is legit
        if (PersonId is Guid personId &&
            (!PotentialDuplicates!.Any(d => d.PersonId == personId) && personId != ResolveTeacherPensionsPotentialDuplicateState.CreateNewRecordPersonIdSentinel))
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
            }
        });

        return Redirect(
            PersonId == ResolveTeacherPensionsPotentialDuplicateState.CreateNewRecordPersonIdSentinel ?
                linkGenerator.TeacherPensionsCheckAnswers(SupportTaskReference!, JourneyInstance!.InstanceId) :
                linkGenerator.TeacherPensionsMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.TeacherPensions());
    }

    protected IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? emailAddress,
        string? nationalInsuranceNumber,
        Gender? gender)
    {
        return Impl().AsReadOnly();

        IEnumerable<PersonMatchedAttribute> Impl()
        {
            var requestData = GetRequestData();

            if (firstName == requestData.FirstName)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (middleName == requestData.MiddleName || (string.IsNullOrWhiteSpace(requestData.MiddleName) && string.IsNullOrWhiteSpace(middleName)))
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (lastName == requestData.LastName)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (dateOfBirth == requestData.DateOfBirth)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (nationalInsuranceNumber == requestData.NationalInsuranceNumber)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }

            if (gender == requestData.Gender)
            {
                yield return PersonMatchedAttribute.Gender;
            }
        }
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RequestData = GetRequestData();

        if (RequestData.PotentialDuplicate != true ||
            RequestData.Matches is not { MatchedPersons.Count: >= 1 })
        {
            context.Result = BadRequest();
            return;
        }

        var matchedPersonIds = RequestData.Matches.MatchedPersons.Select(m => m.PersonId).ToArray();

        PotentialDuplicates = (await dbContext.Persons
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
                HasActiveAlerts = p.Alerts!.Any(a => a.IsOpen),
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
                    r.NationalInsuranceNumber,
                    r.Gender)
            })
            .ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}

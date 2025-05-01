using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

[Journey(JourneyNames.ResolveApiTrnRequest), RequireJourneyInstance, ActivatesJourney]
public class Matches(TrsDbContext dbContext, TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<ResolveApiTrnRequestState>? JourneyInstance { get; set; }

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

        await JourneyInstance!.UpdateStateAsync(state => state.PersonId = PersonId);

        return Redirect(linkGenerator.ApiTrnRequestMerge(SupportTaskReference!, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.ApiTrnRequests());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;

        if (RequestData.PotentialDuplicate != true ||
            RequestData.Matches is not { MatchedRecords.Count: >= 1 })
        {
            context.Result = BadRequest();
            return;
        }

        var matchedPersonIds = RequestData.Matches.MatchedRecords.Select(m => m.PersonId).ToArray();

        PotentialDuplicates = (await dbContext.Persons
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
            .Select((r, i) => r with
            {
                Identifier = (char)('A' + i),
                MatchedAttributes = GetMatchedAttributes(
                    RequestData,
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

    private static IReadOnlyCollection<PersonMatchedAttribute> GetMatchedAttributes(
        TrnRequestMetadata requestMetadata,
        string firstName,
        string middleName,
        string lastName,
        DateOnly? dateOfBirth,
        string? emailAddress,
        string? nationalInsuranceNumber)
    {
        return Impl().AsReadOnly();

        IEnumerable<PersonMatchedAttribute> Impl()
        {
            if (firstName == requestMetadata.FirstName)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (middleName == requestMetadata.MiddleName)
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (lastName == requestMetadata.LastName)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (dateOfBirth == requestMetadata.DateOfBirth)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (emailAddress == requestMetadata.EmailAddress)
            {
                yield return PersonMatchedAttribute.EmailAddress;
            }

            if (nationalInsuranceNumber == requestMetadata.NationalInsuranceNumber)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }
        }
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

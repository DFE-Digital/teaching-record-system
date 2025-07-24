using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.Matches;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.MergePerson), RequireJourneyInstance]
public class CompareMatchingRecordsModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    private static readonly InductionStatus[] _invalidInductionStatusesForMerge = [InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed];

    public string BackLink => GetPageLink(MergeJourneyPage.EnterTrn);

    public string? CannotMergeReason { get; private set; }

    public PotentialDuplicate[]? PotentialDuplicates { get; private set; }

    [Display(Name = "Which is the primary record?")]
    [Required(ErrorMessage = "Select primary record")]
    [BindProperty]
    public Guid? PrimaryRecordId { get; set; }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.PersonAId is not Guid personAId || JourneyInstance!.State.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(MergeJourneyPage.EnterTrn));
            return;
        }

        Guid[] personIds = [personAId, personBId];

        var potentialDuplicates = (await DbContext.Persons
            .IgnoreQueryFilters()
            .Where(p => personIds.Contains(p.PersonId))
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
                ActiveAlertCount = p.Alerts!.Count(a => a.IsOpen),
                InductionStatus = p.InductionStatus,
                Status = p.Status
            })
            .ToArrayAsync())
            .OrderBy(p => Array.IndexOf(personIds, p.PersonId))
            .ToArray();

        PotentialDuplicates = potentialDuplicates
            .Select((r, i) => r with
            {
                Identifier = (char)('A' + i),
                MatchedAttributes = i == 0
                    ? [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.MiddleName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber
                    ]
                    : GetPersonAttributeMatches(
                        potentialDuplicates[0],
                        r.FirstName,
                        r.MiddleName,
                        r.LastName,
                        r.DateOfBirth,
                        r.EmailAddress,
                        r.NationalInsuranceNumber)
            })
            .ToArray();

        foreach (var potentialDuplicate in PotentialDuplicates)
        {
            var hasBeenDeactivated = potentialDuplicate.Status == PersonStatus.Deactivated;
            var hasActiveAlerts = potentialDuplicate.ActiveAlertCount > 0;
            var invalidStatus = _invalidInductionStatusesForMerge
                .Cast<InductionStatus?>()
                .FirstOrDefault(s => potentialDuplicate.InductionStatus == s);

            if (hasBeenDeactivated)
            {
                CannotMergeReason = "One of these records has been deactivated.";
                break;
            }

            if (hasActiveAlerts && invalidStatus is not null)
            {
                CannotMergeReason = $"One of these records has an alert and an induction status of {invalidStatus.GetDisplayName()}.";
                break;
            }

            if (hasActiveAlerts)
            {
                CannotMergeReason = "One of these records has an alert.";
                break;
            }

            if (invalidStatus is not null)
            {
                CannotMergeReason = $"The induction status of one of these records is {invalidStatus.GetDisplayName()}.";
                break;
            }
        }
    }

    public IActionResult OnGet()
    {
        PrimaryRecordId = JourneyInstance!.State.PrimaryRecordId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CannotMergeReason is not null)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.PrimaryRecordId = PrimaryRecordId;
        });

        return Redirect(GetPageLink(MergeJourneyPage.SelectDetailsToMerge));
    }

    protected IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
        PotentialDuplicate recordToMatchAgainst,
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
            if (firstName == recordToMatchAgainst.FirstName)
            {
                yield return PersonMatchedAttribute.FirstName;
            }

            if (middleName == recordToMatchAgainst.MiddleName)
            {
                yield return PersonMatchedAttribute.MiddleName;
            }

            if (lastName == recordToMatchAgainst.LastName)
            {
                yield return PersonMatchedAttribute.LastName;
            }

            if (dateOfBirth == recordToMatchAgainst.DateOfBirth)
            {
                yield return PersonMatchedAttribute.DateOfBirth;
            }

            if (emailAddress == recordToMatchAgainst.EmailAddress)
            {
                yield return PersonMatchedAttribute.EmailAddress;
            }

            if (nationalInsuranceNumber == recordToMatchAgainst.NationalInsuranceNumber)
            {
                yield return PersonMatchedAttribute.NationalInsuranceNumber;
            }
        }
    }
}

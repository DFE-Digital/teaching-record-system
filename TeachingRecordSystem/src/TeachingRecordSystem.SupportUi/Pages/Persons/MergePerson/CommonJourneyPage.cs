using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public abstract class CommonJourneyPage(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<MergePersonState>? JourneyInstance { get; set; }

    protected TrsDbContext DbContext { get; } = dbContext;
    protected SupportUiLinkGenerator LinkGenerator { get; } = linkGenerator;
    protected EvidenceUploadManager EvidenceUploadManager { get; } = evidenceUploadManager;

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string CancelLink => LinkGenerator.Persons.MergePerson.EnterTrnCancel(PersonId, JourneyInstance!.InstanceId);

    public string GetPageLink(MergePersonJourneyPage? pageName, bool? fromCheckAnswers = null)
    {
        fromCheckAnswers ??= FromCheckAnswers ? true : null;
        return pageName switch
        {
            MergePersonJourneyPage.EnterTrn => LinkGenerator.Persons.MergePerson.EnterTrn(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            MergePersonJourneyPage.Matches => LinkGenerator.Persons.MergePerson.Matches(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            MergePersonJourneyPage.Merge => LinkGenerator.Persons.MergePerson.Merge(PersonId, JourneyInstance!.InstanceId, fromCheckAnswers),
            MergePersonJourneyPage.CheckAnswers => LinkGenerator.Persons.MergePerson.CheckAnswers(PersonId, JourneyInstance!.InstanceId),
            _ => LinkGenerator.Persons.PersonDetail.Index(PersonId)
        };
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        OnPageHandlerExecuting(context);
        await OnPageHandlerExecutingAsync(context);
        if (context.Result == null)
        {
            var executedContext = await next();
            OnPageHandlerExecuted(executedContext);
            await OnPageHandlerExecutedAsync(executedContext);
        }
    }

    protected virtual async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(PersonId, async () => (await DbContext.Persons.SingleAsync(q => q.PersonId == PersonId)).Trn);
    }

    protected virtual Task OnPageHandlerExecutedAsync(PageHandlerExecutedContext context)
        => Task.CompletedTask;

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await EvidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(GetPageLink(null));
    }

    protected IReadOnlyCollection<PersonMatchedAttribute> GetPersonAttributeMatches(
        PersonAttributes recordToMatchAgainst,
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

            if (gender == recordToMatchAgainst.Gender)
            {
                yield return PersonMatchedAttribute.Gender;
            }
        }
    }

    protected async Task<IReadOnlyList<PotentialDuplicate>> GetPotentialDuplicatesAsync(params Guid[] personIds)
    {
        var potentialDuplicates = (await DbContext.Persons
            .IgnoreQueryFilters()
            .Where(p => personIds.Contains(p.PersonId))
            .Select(p => new PotentialDuplicate
            {
                Trn = p.Trn!,
                PersonId = p.PersonId,
                Identifier = 'X', // We'll fix this below, can't do it over an IQueryable
                MatchedAttributes = Array.Empty<PersonMatchedAttribute>(),  // ditto
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                EmailAddress = p.EmailAddress,
                NationalInsuranceNumber = p.NationalInsuranceNumber,
                Gender = p.Gender,
                Status = p.Status,
                InductionStatus = p.InductionStatus,
                ActiveAlertCount = p.Alerts!.Count(a => a.IsOpen && a.DeletedOn == null),
                Attributes = new PersonAttributes
                {
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    EmailAddress = p.EmailAddress,
                    NationalInsuranceNumber = p.NationalInsuranceNumber,
                    Gender = p.Gender
                }
            })
            .ToArrayAsync())
            .OrderBy(p => Array.IndexOf(personIds, p.PersonId))
            .ToArray();

        return potentialDuplicates
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
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ]
                    : GetPersonAttributeMatches(
                        potentialDuplicates[0].Attributes,
                        r.FirstName,
                        r.MiddleName,
                        r.LastName,
                        r.DateOfBirth,
                        r.EmailAddress,
                        r.NationalInsuranceNumber,
                        r.Gender)
            })
            .ToArray();
    }
}

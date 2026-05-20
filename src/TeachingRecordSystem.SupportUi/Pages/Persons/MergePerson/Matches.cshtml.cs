using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson), RequireJourneyInstance]
public class MatchesModel(
    TrsDbContext dbContext,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceUploadManager)
{
    private readonly InlineValidator<MatchesModel> _validator = new()
    {
        v => v.RuleFor(m => m.PrimaryPersonId)
            .NotNull().WithMessage("Select primary record")
    };

    public string BackLink => GetPageLink(FromCheckAnswers ? MergePersonJourneyPage.CheckAnswers : MergePersonJourneyPage.EnterTrn);

    public string? CannotMergeReason { get; private set; }

    public IReadOnlyList<PotentialDuplicate>? PotentialDuplicates { get; private set; }

    [BindProperty]
    public Guid? PrimaryPersonId { get; set; }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.PersonAId is not Guid personAId || JourneyInstance!.State.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(MergePersonJourneyPage.EnterTrn));
            return;
        }

        PotentialDuplicates = await GetPotentialDuplicatesAsync(personAId, personBId);

        foreach (var potentialDuplicate in PotentialDuplicates)
        {
            if (potentialDuplicate.HasBeenDeactivated)
            {
                CannotMergeReason = "One of these records has been deactivated";
                break;
            }

            if (potentialDuplicate.HasActiveAlerts && potentialDuplicate.HasInvalidInductionStatus)
            {
                CannotMergeReason = $"One of these records has an alert and an induction status of {potentialDuplicate.InductionStatus.GetDisplayName()}";
                break;
            }

            if (potentialDuplicate.HasActiveAlerts)
            {
                CannotMergeReason = "One of these records has an alert";
                break;
            }

            if (potentialDuplicate.HasInvalidInductionStatus)
            {
                CannotMergeReason = $"The induction status of one of these records is {potentialDuplicate.InductionStatus.GetDisplayName()}";
                break;
            }
        }
    }

    public IActionResult OnGet()
    {
        PrimaryPersonId = JourneyInstance!.State.PrimaryPersonId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PotentialDuplicates!.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        _validator.ValidateAndThrow(this);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // If primary record changes and attribute sources already selected, we assume the selected data for each attribute should be kept the same,
            // so we need to swap the sources for the selected attributes.
            if (state.PersonAttributeSourcesSet &&
                state.PrimaryPersonId is Guid originalPrimaryPersonId &&
                PrimaryPersonId is Guid newPrimaryPersonId &&
                originalPrimaryPersonId != newPrimaryPersonId)
            {
                state.FirstNameSource = state.FirstNameSource is not PersonAttributeSource firstNameSource ? null : firstNameSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
                state.MiddleNameSource = state.MiddleNameSource is not PersonAttributeSource middleNameSource ? null : middleNameSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
                state.LastNameSource = state.LastNameSource is not PersonAttributeSource lastNameSource ? null : lastNameSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
                state.DateOfBirthSource = state.DateOfBirthSource is not PersonAttributeSource dateOfBirthSource ? null : dateOfBirthSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
                state.EmailAddressSource = state.EmailAddressSource is not PersonAttributeSource emailAddressSource ? null : emailAddressSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
                state.NationalInsuranceNumberSource = state.NationalInsuranceNumberSource is not PersonAttributeSource nationalInsuranceNumberSource ? null : nationalInsuranceNumberSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
                state.GenderSource = state.GenderSource is not PersonAttributeSource genderSource ? null : genderSource == PersonAttributeSource.PrimaryPerson ? PersonAttributeSource.SecondaryPerson : PersonAttributeSource.PrimaryPerson;
            }
            state.PrimaryPersonId = PrimaryPersonId;
        });

        return Redirect(GetPageLink(FromCheckAnswers ? MergePersonJourneyPage.CheckAnswers : MergePersonJourneyPage.Merge));
    }
}

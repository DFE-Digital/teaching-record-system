using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

[Journey(JourneyNames.MergePerson)]
public class MatchesModel(
    MergePersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<MatchesModel> _validator = new()
    {
        v => v.RuleFor(m => m.PrimaryPersonId)
            .NotNull().WithMessage("Select primary record")
    };

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    public string? CannotMergeReason { get; private set; }

    public IReadOnlyList<PotentialDuplicate>? PotentialDuplicates { get; private set; }

    [BindProperty]
    public Guid? PrimaryPersonId { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink();

        PotentialDuplicates = await journey.GetPotentialDuplicatesAsync(journey.State.PersonAId!.Value, journey.State.PersonBId!.Value);

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

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public IActionResult OnGet()
    {
        PrimaryPersonId = journey.State.PrimaryPersonId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        if (PotentialDuplicates!.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.MergePerson.Merge(journey.InstanceId),
            state =>
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
    }
}

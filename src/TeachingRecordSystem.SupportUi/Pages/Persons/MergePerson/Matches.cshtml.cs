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

    public string? CannotMergeReason { get; private set; }

    public IReadOnlyList<PotentialDuplicate>? PotentialDuplicates { get; private set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? PrimaryPersonId { get; set; }

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

        _validator.ValidateAndThrow(this);

        return journey.AdvanceTo(
            linkGenerator.Persons.MergePerson.Merge(journey.InstanceId),
            state =>
            {
                // If the primary record changes, we assume the data selected for each attribute should stay
                // the same, so we swap the source of every attribute that's already been chosen.
                if (state.PrimaryPersonId is Guid originalPrimaryPersonId &&
                    PrimaryPersonId is Guid newPrimaryPersonId &&
                    originalPrimaryPersonId != newPrimaryPersonId)
                {
                    state.FirstNameSource = Swap(state.FirstNameSource);
                    state.MiddleNameSource = Swap(state.MiddleNameSource);
                    state.LastNameSource = Swap(state.LastNameSource);
                    state.DateOfBirthSource = Swap(state.DateOfBirthSource);
                    state.EmailAddressSource = Swap(state.EmailAddressSource);
                    state.NationalInsuranceNumberSource = Swap(state.NationalInsuranceNumberSource);
                    state.GenderSource = Swap(state.GenderSource);
                }

                state.PrimaryPersonId = PrimaryPersonId;
            });

        static PersonAttributeSource? Swap(PersonAttributeSource? source) => source switch
        {
            PersonAttributeSource.PrimaryPerson => PersonAttributeSource.SecondaryPerson,
            PersonAttributeSource.SecondaryPerson => PersonAttributeSource.PrimaryPerson,
            _ => null
        };
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        BackLink = journey.GetBackLink();

        PotentialDuplicates = await journey.GetPotentialDuplicatesAsync(journey.State.PersonAId, journey.State.PersonBId!.Value);

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
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;

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

    public IReadOnlyList<MergeCandidate>? MergeCandidates { get; private set; }

    [BindProperty]
    public Guid? PrimaryPersonId { get; set; }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var cancellationToken = context.HttpContext.RequestAborted;

        BackLink = journey.GetBackLink();

        MergeCandidates = await journey.GetMergeCandidatesAsync([journey.State.PersonAId!.Value, journey.State.PersonBId!.Value], cancellationToken);

        // Some blockers depend on which record is retained; only refuse outright if neither record can be the primary
        var blockersWithAAsPrimary = new MergePair(MergeCandidates[0], MergeCandidates[1]).MergeBlockers;
        var blockersWithBAsPrimary = new MergePair(MergeCandidates[1], MergeCandidates[0]).MergeBlockers;

        if (blockersWithAAsPrimary is not PersonMergeBlockers.None && blockersWithBAsPrimary is not PersonMergeBlockers.None)
        {
            CannotMergeReason = GetCannotMergeReason(blockersWithAAsPrimary, blockersWithBAsPrimary);
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public IActionResult OnGet()
    {
        PrimaryPersonId = journey.State.PrimaryPersonId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync(cancellationToken));
        }

        if (CannotMergeReason is not null)
        {
            return BadRequest();
        }

        await this.ThrowIfInvalidAsync(_validator, cancellationToken);

        var primaryPerson = MergeCandidates!.SingleOrDefault(c => c.PersonId == PrimaryPersonId);

        if (primaryPerson is null)
        {
            return BadRequest();
        }

        var secondaryPerson = MergeCandidates!.Single(c => c.PersonId != PrimaryPersonId);
        var blockers = new MergePair(primaryPerson, secondaryPerson).MergeBlockers;

        if (blockers is not PersonMergeBlockers.None)
        {
            ModelState.AddModelError(nameof(PrimaryPersonId), GetInvalidPrimaryPersonReason(blockers));
            return this.PageWithErrors();
        }

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

    private static string GetCannotMergeReason(PersonMergeBlockers blockersWithAAsPrimary, PersonMergeBlockers blockersWithBAsPrimary) =>
        (blockersWithAAsPrimary & blockersWithBAsPrimary) switch
        {
            var b when b.HasFlag(PersonMergeBlockers.Deactivated) =>
                "One of these records has been deactivated",
            var b when b.HasFlag(PersonMergeBlockers.InductionStatusFailed) =>
                "One of these records has an induction status of failed",
            var b when b.HasFlag(PersonMergeBlockers.InductionStartDateMismatch) =>
                "The induction start dates do not match",
            var b when b.HasFlag(PersonMergeBlockers.InductionCompletedDateMismatch) =>
                "The induction completed dates do not match",
            var b when b.HasFlag(PersonMergeBlockers.OpenAlert) =>
                "Both records have an alert",
            var b when b.HasFlag(PersonMergeBlockers.InductionStatusFailedInWales) =>
                "Both records have an induction status of failed in Wales",
            // Each record is blocked from being deactivated for a different reason
            _ => "One of these records has an alert and the other has an induction status of failed in Wales"
        };

    private static string GetInvalidPrimaryPersonReason(PersonMergeBlockers blockers) =>
        blockers switch
        {
            var b when b.HasFlag(PersonMergeBlockers.OpenAlert) =>
                "The secondary record has an alert",
            var b when b.HasFlag(PersonMergeBlockers.InductionStatusFailedInWales) =>
                "The induction status of the secondary record is failed in Wales",
            _ => throw new InvalidOperationException($"Unexpected merge blockers: {blockers}.")
        };
}

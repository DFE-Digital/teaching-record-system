using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.InductionExemptions;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class ExemptionReasonsModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceUploadManager evidenceController,
    InductionExemptionService inductionExemptionService)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    public Guid[] ExemptionReasonIds { get; set; } = [];

    public Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> ExemptionReasons { get; set; } = new();

    protected IEnumerable<RouteWithExemption>? RoutesWithInductionExemptions { get; private set; }

    public bool ShowInductionExemptionReasonNotAvailableMessage =>
        RoutesWithInductionExemptions?
            .Any(r => InductionExemptionService.ExemptionsToBeExcludedIfRouteQualificationIsHeld.Contains(r.InductionExemptionReasonId)) ?? false;

    public string[]? InductionExemptionFromRoutesMessages
    {
        get
        {
            if (RoutesWithInductionExemptions is null || !RoutesWithInductionExemptions.Any())
            {
                return null;
            }
            else
            {
                List<string> messages = [];
                foreach (var route in RoutesWithInductionExemptions!)
                {
                    messages.Add($"This person has an induction exemption \"{route.InductionExemptionReasonName}\" on the \"{route.RouteToProfessionalStatusName}\" route.");
                }
                return messages.ToArray();
            }
        }
    }

    public string[]? InductionExemptionReasonNotAvailableMessages
    {
        get
        {
            if (!ShowInductionExemptionReasonNotAvailableMessage)
            {
                return null;
            }
            else
            {
                List<string> messages = [];
                foreach (var route in RoutesWithInductionExemptions!
                    .Where(r => InductionExemptionService.ExemptionsToBeExcludedIfRouteQualificationIsHeld.Contains(r.InductionExemptionReasonId)))
                {
                    messages.Add($"To add/remove the induction exemption reason of: \"{route.InductionExemptionReasonName}\" please modify the \"{route.RouteToProfessionalStatusName}\" route.");
                }
                return messages.ToArray();
            }
        }
    }


    public InductionJourneyPage NextPage
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckAnswersPage.CheckAnswers)
            {
                return InductionJourneyPage.CheckAnswers;
            }
            return InductionJourneyPage.ChangeReasons;
        }
    }

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckAnswersPage.CheckAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.ExemptionReason
                ? LinkGenerator.Persons.PersonDetail.Induction(PersonId)
                : GetPageLink(InductionJourneyPage.Status);
        }
    }

    public IActionResult OnGet()
    {
        if (JourneyInstance!.State.ExemptionReasonIds != null)
        {
            ExemptionReasonIds = JourneyInstance!.State.ExemptionReasonIds;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ExemptionReasonIds.Length == 0)
        {
            ModelState.AddModelError(nameof(ExemptionReasonIds), "Select the reason for a teacher’s exemption to induction");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ExemptionReasonIds = ExemptionReasonIds;
        });

        return Redirect(GetPageLink(NextPage));
    }

    protected override InductionJourneyPage StartPage => InductionJourneyPage.ExemptionReason;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.InductionStatus != InductionStatus.Exempt)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }

        var response = await inductionExemptionService.GetExemptionReasonsAsync(PersonId);

        RoutesWithInductionExemptions = response.RoutesWithInductionExemptions;
        ExemptionReasons = response.ExemptionReasonCategories;
    }
}

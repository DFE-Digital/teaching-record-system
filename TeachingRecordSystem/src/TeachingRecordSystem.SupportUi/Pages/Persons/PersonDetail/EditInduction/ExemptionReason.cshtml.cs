using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class ExemptionReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Display(Name = "Why are they exempt from induction?")]
    public Guid[] ExemptionReasonIds { get; set; } = [];

    public InductionExemptionReason[] ExemptionReasons { get; set; } = [];

    public IEnumerable<ProfessionalStatus>? ExemptedRoutes;

    public bool ShowInductionExemptionReasonNotAvailableMessage => ExemptedRoutes?
        .Any(r => r.RouteToProfessionalStatusId == RouteToProfessionalStatus.ScotlandRId || r.RouteToProfessionalStatusId == RouteToProfessionalStatus.NiRId) ?? false;

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
                List<string> messages = new();
                foreach (var exemptedRoute in ExemptedRoutes!.Where(r => r.RouteToProfessionalStatusId == RouteToProfessionalStatus.ScotlandRId || r.RouteToProfessionalStatusId == RouteToProfessionalStatus.NiRId))
                {
                    messages.Add($"To add/remove the Induction exemption reason of: \"{exemptedRoute.RouteToProfessionalStatus?.InductionExemptionReason?.Name}\" please modify the \"{exemptedRoute.RouteToProfessionalStatus?.Name}\" route");
                }
                return messages.ToArray();
            }
        }
    }


    public InductionJourneyPage NextPage
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
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
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.ExemptionReason
                ? LinkGenerator.PersonInduction(PersonId)
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

        ExemptedRoutes = DbContext.ProfessionalStatuses
            .Include(p => p.RouteToProfessionalStatus)
            .ThenInclude(r => r != null ? r.InductionExemptionReason : null)
            .Where(p => p.PersonId == PersonId && p.RouteToProfessionalStatus != null && p.RouteToProfessionalStatus.InductionExemptionReason != null);

        ExemptionReasons = (await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Where(e => !ExemptedRoutes?.Select(r => r.RouteToProfessionalStatus?.InductionExemptionReasonId).Contains(e.InductionExemptionReasonId) ?? true) // CML TODO check / tidy
            .ToArray();
    }
}

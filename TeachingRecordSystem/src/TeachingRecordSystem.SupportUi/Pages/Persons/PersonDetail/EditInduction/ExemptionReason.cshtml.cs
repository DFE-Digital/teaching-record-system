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
    IFileService fileService,
    IFeatureProvider featureProvider)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    protected class RouteWithExemption
    {
        public required Guid RouteToProfessionalStatusId { get; init; }
        public required Guid InductionExemptionReasonId { get; init; }
        public required string RouteToProfessionalStatusName { get; init; }
        public required string InductionExemptionReasonName { get; init; }
    }

    [BindProperty]
    [Display(Name = "Why are they exempt from induction?")]
    public Guid[] ExemptionReasonIds { get; set; } = [];

    public Dictionary<ExemptionReasonCategory, IEnumerable<InductionExemptionReason>> ExemptionReasons { get; set; } = new();

    protected IEnumerable<RouteWithExemption>? RoutesWithInductionExemptions;

    public bool ShowInductionExemptionReasonNotAvailableMessage => featureProvider.IsEnabled(FeatureNames.RoutesToProfessionalStatus) &&
        (RoutesWithInductionExemptions?
        .Any(r => ExemptionReasonCategories.ExemptionsToBeExcludedIfRouteQualificationIsHeld.Contains(r.InductionExemptionReasonId)) ?? false);

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
                List<string> messages = new();
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
                List<string> messages = new();
                foreach (var route in RoutesWithInductionExemptions!
                    .Where(r => ExemptionReasonCategories.ExemptionsToBeExcludedIfRouteQualificationIsHeld.Contains(r.InductionExemptionReasonId)))
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

        var exemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);

        if (featureProvider.IsEnabled(FeatureNames.RoutesToProfessionalStatus))
        {
            RoutesWithInductionExemptions = DbContext.ProfessionalStatuses
                .Include(p => p.RouteToProfessionalStatusType)
                .ThenInclude(r => r!.InductionExemptionReason)
                .Where(
                    p => p.PersonId == PersonId &&
                    p.RouteToProfessionalStatusType != null &&
                    p.ExemptFromInduction == true &&
                    p.RouteToProfessionalStatusType.InductionExemptionReason != null)
                .Select(r => new RouteWithExemption()
                {
                    InductionExemptionReasonId = r.RouteToProfessionalStatusType!.InductionExemptionReasonId!.Value,
                    RouteToProfessionalStatusId = r.RouteToProfessionalStatusTypeId,
                    InductionExemptionReasonName = r.RouteToProfessionalStatusType.InductionExemptionReason!.Name,
                    RouteToProfessionalStatusName = r.RouteToProfessionalStatusType.Name
                });
        }
        if (RoutesWithInductionExemptions is not null && RoutesWithInductionExemptions.Any())
        {
            var exemptionReasonIdsToExclude = ExemptionReasonCategories.ExemptionsToBeExcludedIfRouteQualificationIsHeld
                .Join(RoutesWithInductionExemptions,
                    guid => guid,
                    r => r.InductionExemptionReasonId,
                    (guid, route) => route.InductionExemptionReasonId);

            var exemptionReasonsToDisplay = ExemptionReasonCategories.ExemptionReasonIds
                .Where(id => !exemptionReasonIdsToExclude.Contains(id))
                .Join(exemptionReasons,
                    guid => guid,
                    exemption => exemption.InductionExemptionReasonId,
                    (guid, exemption) => exemption)
                .ToArray();

            ExemptionReasons = ExemptionReasonCategories.CreateFilteredDictionaryFromIds(exemptionReasonsToDisplay);
        }
        else
        {
            var exemptionReasonsToDisplay = ExemptionReasonCategories.ExemptionReasonIds
                .Join(exemptionReasons,
                    guid => guid,
                    exemption => exemption.InductionExemptionReasonId,
                    (guid, exemption) => exemption)
                .ToArray();

            ExemptionReasons = ExemptionReasonCategories.CreateFilteredDictionaryFromIds(exemptionReasonsToDisplay);
        }
    }
}

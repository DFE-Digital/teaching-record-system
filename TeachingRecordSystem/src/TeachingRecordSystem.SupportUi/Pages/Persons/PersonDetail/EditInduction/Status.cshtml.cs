using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, IClock clock)
    : CommonJourneyPage(linkGenerator)
{
    private const string InductionIsManagedByCpdWarning = "To change this teacher’s induction status to passed, failed, or in progress, use the Record inductions as an appropriate body service.";

    private bool _inductionStatusManagedByCpd;

    [FromQuery]
    public JourneyFromCheckYourAnswersPage? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "What is their induction status?")]
    [NotEqual(InductionStatus.None, ErrorMessage = "Select a status")]
    public InductionStatus InductionStatus { get; set; }
    public string? PersonName { get; set; }
    public IEnumerable<InductionStatusInfo> StatusChoices
    {
        get
        {
            return _inductionStatusManagedByCpd ?
                 InductionStatusRegistry.ValidStatusChangesWhenManagedByCpd
                : InductionStatusRegistry.All.ToArray()[1..];
        }
    }
    public string? StatusWarningMessage
    {
        get
        {
            if (_inductionStatusManagedByCpd)
            {
                return InductionIsManagedByCpdWarning;
            }
            else
            {
                return null;
            }
        }
    }

    public InductionJourneyPage NextPage
    {
        get
        {
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresExemptionReasons() => InductionJourneyPage.ExemptionReason,
                _ when InductionStatus.RequiresStartDate() => InductionJourneyPage.StartDate,
                _ => InductionJourneyPage.ChangeReasons
            };
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
            return LinkGenerator.PersonInduction(PersonId);
        }
    }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.InductionStatus = InductionStatus;
            if (!InductionStatus.RequiresStartDate())
            {
                state.StartDate = null;
            }
            if (!InductionStatus.RequiresCompletedDate())
            {
                state.CompletedDate = null;
            }
            if (!InductionStatus.RequiresExemptionReasons())
            {
                state.ExemptionReasonIds = Array.Empty<Guid>();
            }
        });

        return Redirect(GetPageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(dbContext, PersonId, InductionJourneyPage.Status);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == PersonId);
        _inductionStatusManagedByCpd = person.InductionStatusManagedByCpd(clock.Today);

        await next();
    }
}

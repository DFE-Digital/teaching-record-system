using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel : CommonJourneyPage
{
    private const string InductionIsManagedByCpdWarning = "To change this teacher’s induction status to passed, failed, or in progress, use the Record inductions as an appropriate body service.";

    protected TrsDbContext _dbContext;
    protected IClock _clock;
    protected bool InductionStatusManagedByCpd;

    [BindProperty]
    [Display(Name = "Select a status")]
    [NotEqual(InductionStatus.None, ErrorMessage = "Select a status")]
    public InductionStatus InductionStatus { get; set; }
    public InductionStatus CurrentInductionStatus { get; set; }
    public string? PersonName { get; set; }
    public IEnumerable<InductionStatusInfo> StatusChoices
    {
        get
        {
            return InductionStatusManagedByCpd ?
                 InductionStatusRegistry.ValidStatusChangesWhenManagedByCpd.Where(i => i.Value != CurrentInductionStatus)
                : InductionStatusRegistry.All.ToArray()[1..].Where(i => i.Value != CurrentInductionStatus);
        }
    }
    public string? StatusWarningMessage
    {
        get
        {
            if (InductionStatusManagedByCpd)
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

    public string BackLink => LinkGenerator.PersonInduction(PersonId);

    public StatusModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, IClock clock) : base(linkGenerator)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task OnGetAsync()
    {
        var person = await _dbContext.Persons.SingleAsync(q => q.PersonId == PersonId);
        InductionStatusManagedByCpd = person.InductionStatusManagedByCpd(_clock.Today);
        InductionStatus = JourneyInstance!.State.InductionStatus;
        CurrentInductionStatus = JourneyInstance!.State.CurrentInductionStatus;
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            if (state.CurrentInductionStatus == InductionStatus.None)
            {
                state.CurrentInductionStatus = CurrentInductionStatus;
            }
        });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var person = await _dbContext.Persons.SingleAsync(q => q.PersonId == PersonId);
            InductionStatusManagedByCpd = person.InductionStatusManagedByCpd(_clock.Today);
            CurrentInductionStatus = JourneyInstance!.State.CurrentInductionStatus;
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.InductionStatus = InductionStatus;
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.Status;
            }
        });

        return Redirect(PageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.Status);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        await next();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class CheckAnswersModel(TrsDbContext dbContext, TrsLinkGenerator linkGenerator, IClock clock) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public Guid ProviderId { get; set; }

    public string? ProviderName { get; set; }

    public MandatoryQualificationSpecialism Specialism { get; set; }

    public DateOnly StartDate { get; set; }

    public MandatoryQualificationStatus Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var qualification = new MandatoryQualification()
        {
            QualificationId = Guid.NewGuid(),
            CreatedOn = clock.UtcNow,
            UpdatedOn = clock.UtcNow,
            PersonId = PersonId,
            ProviderId = ProviderId,
            Status = Status,
            Specialism = Specialism,
            StartDate = StartDate,
            EndDate = EndDate
        };
        dbContext.MandatoryQualifications.Add(qualification);

        var createdEvent = new MandatoryQualificationCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId(),
            PersonId = PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(qualification, providerNameHint: ProviderName)
        };
        dbContext.AddEvent(createdEvent);

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification added");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqAddProvider(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        ProviderId = JourneyInstance.State.ProviderId.Value;
        ProviderName = MandatoryQualificationProvider.GetById(ProviderId).Name;
        Specialism = JourneyInstance.State.Specialism.Value;
        StartDate = JourneyInstance.State.StartDate.Value;
        Status = JourneyInstance.State.Status.Value;
        EndDate = JourneyInstance.State.EndDate;

        await next();
    }
}

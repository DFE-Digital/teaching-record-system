using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class CheckAnswersModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public dfeta_mqestablishment? MqEstablishment { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public DateOnly? StartDate { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? EndDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var qualificationId = Guid.NewGuid();

        var mqSpecialism = await referenceDataCache.GetMqSpecialismByValue(Specialism!.Value.GetDqtValue());

        var createdEvent = new MandatoryQualificationCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = clock.UtcNow,
            RaisedBy = User.GetUserId(),
            PersonId = PersonId,
            MandatoryQualification = new()
            {
                QualificationId = qualificationId,
                Provider = MqEstablishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = null,
                        Name = null,
                        DqtMqEstablishmentId = MqEstablishment?.Id,
                        DqtMqEstablishmentName = MqEstablishment?.dfeta_name
                    } :
                    null,
                Specialism = Specialism,
                Status = Status,
                StartDate = StartDate,
                EndDate = EndDate
            }
        };

        await crmQueryDispatcher.ExecuteQuery(
            new CreateMandatoryQualificationQuery()
            {
                QualificationId = qualificationId,
                ContactId = PersonId,
                MqEstablishmentId = MqEstablishment!.Id,
                SpecialismId = mqSpecialism.Id,
                StartDate = StartDate!.Value,
                Status = Status!.Value.GetDqtStatus(),
                EndDate = Status == MandatoryQualificationStatus.Passed
                    ? EndDate!.Value
                    : null,
                Event = createdEvent
            });

        await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(
            helper => helper.SyncMandatoryQualification(qualificationId, new[] { createdEvent }, CancellationToken.None));

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification added");

        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostCancel()
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
        MqEstablishment = await referenceDataCache.GetMqEstablishmentByValue(JourneyInstance!.State.MqEstablishmentValue!);
        Specialism = JourneyInstance.State.Specialism;
        StartDate = JourneyInstance.State.StartDate;
        Status = JourneyInstance.State.Status;
        EndDate = JourneyInstance.State.EndDate;

        await next();
    }
}

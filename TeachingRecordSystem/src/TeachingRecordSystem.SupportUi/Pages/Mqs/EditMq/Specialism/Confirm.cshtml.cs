using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[Journey(JourneyNames.EditMqSpecialism), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock) : PageModel
{
    public JourneyInstance<EditMqSpecialismState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public MandatoryQualificationSpecialism? CurrentSpecialism { get; set; }

    public MandatoryQualificationSpecialism? NewSpecialism { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var qualification = (await crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId)))!;
        var mqSpecialism = await referenceDataCache.GetMqSpecialismByValue(NewSpecialism!.Value.GetDqtValue());

        var changes = mqSpecialism.Id != qualification.dfeta_MQ_SpecialismId?.Id ?
            MandatoryQualificationUpdatedEventChanges.Specialism :
            MandatoryQualificationUpdatedEventChanges.None;

        if (changes != MandatoryQualificationUpdatedEventChanges.None)
        {
            var establishment = qualification.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
                await referenceDataCache.GetMqEstablishmentById(establishmentId) :
                null;

            var specialism = qualification.dfeta_MQ_SpecialismId?.Id is Guid specialismId ?
                await referenceDataCache.GetMqSpecialismById(specialismId) :
                null;

            var oldMqEventModel = new Core.Events.Models.MandatoryQualification()
            {
                QualificationId = QualificationId,
                Provider = establishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = null,
                        Name = null,
                        DqtMqEstablishmentId = establishment?.Id,
                        DqtMqEstablishmentName = establishment?.dfeta_name
                    } :
                    null,
                Specialism = specialism?.ToMandatoryQualificationSpecialism(),
                Status = qualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus(),
                StartDate = qualification.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = qualification.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            };

            var updatedEvent = new MandatoryQualificationUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = clock.UtcNow,
                RaisedBy = User.GetUserId(),
                PersonId = PersonId!.Value,
                MandatoryQualification = oldMqEventModel with
                {
                    Specialism = NewSpecialism
                },
                OldMandatoryQualification = oldMqEventModel,
                Changes = changes
            };

            await crmQueryDispatcher.ExecuteQuery(
                new UpdateMandatoryQualificationSpecialismQuery(
                    QualificationId,
                    mqSpecialism!.Id,
                    updatedEvent));

            await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(
                helper => helper.SyncMandatoryQualification(QualificationId, new[] { updatedEvent }, CancellationToken.None));
        }

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification changed");

        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqEditSpecialism(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentSpecialism = JourneyInstance!.State.CurrentSpecialism;
        NewSpecialism = JourneyInstance!.State.Specialism;
    }
}

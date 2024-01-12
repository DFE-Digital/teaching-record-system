using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[Journey(JourneyNames.EditMqStartDate), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock) : PageModel
{
    public JourneyInstance<EditMqStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? NewStartDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var qualification = (await crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId)))!;

        var changes = NewStartDate != qualification.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true) ?
            MandatoryQualificationUpdatedEventChanges.StartDate :
            MandatoryQualificationUpdatedEventChanges.None;

        if (changes != MandatoryQualificationUpdatedEventChanges.None)
        {
            var establishment = qualification.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
                await referenceDataCache.GetMqEstablishmentById(establishmentId) :
                null;

            MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishment, out var provider);

            var specialism = qualification.dfeta_MQ_SpecialismId?.Id is Guid specialismId ?
                await referenceDataCache.GetMqSpecialismById(specialismId) :
                null;

            var oldMqEventModel = new Core.Events.Models.MandatoryQualification()
            {
                QualificationId = QualificationId,
                Provider = provider is not null || establishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId,
                        Name = provider?.Name,
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
                    StartDate = NewStartDate
                },
                OldMandatoryQualification = oldMqEventModel,
                Changes = changes
            };

            await crmQueryDispatcher.ExecuteQuery(
                new UpdateMandatoryQualificationStartDateQuery(
                    QualificationId,
                    NewStartDate!.Value,
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
            context.Result = Redirect(linkGenerator.MqEditStartDate(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStartDate = JourneyInstance!.State.CurrentStartDate;
        NewStartDate ??= JourneyInstance!.State.StartDate;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[Journey(JourneyNames.EditMqProvider), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock) : PageModel
{
    public JourneyInstance<EditMqProviderState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? CurrentMqEstablishmentName { get; set; }

    public dfeta_mqestablishment? NewMqEstablishment { get; set; }

    public async Task<IActionResult> OnPost()
    {
        var qualification = (await crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId)))!;

        var changes = NewMqEstablishment?.Id != qualification.dfeta_MQ_MQEstablishmentId?.Id ?
            MandatoryQualificationUpdatedEventChanges.Provider :
            MandatoryQualificationUpdatedEventChanges.None;

        if (changes != MandatoryQualificationUpdatedEventChanges.None)
        {
            var oldEstablishment = qualification.dfeta_MQ_MQEstablishmentId?.Id is Guid oldEstablishmentId ?
                await referenceDataCache.GetMqEstablishmentById(oldEstablishmentId) :
                null;

            MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(oldEstablishment, out var oldProvider);
            MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(NewMqEstablishment, out var newProvider);

            var specialism = qualification.dfeta_MQ_SpecialismId?.Id is Guid specialismId ?
                await referenceDataCache.GetMqSpecialismById(specialismId) :
                null;

            var oldMqEventModel = new Core.Events.Models.MandatoryQualification()
            {
                QualificationId = QualificationId,
                Provider = oldProvider is not null || oldEstablishment is not null ?
                    new()
                    {
                        MandatoryQualificationProviderId = oldProvider?.MandatoryQualificationProviderId,
                        Name = oldProvider?.Name,
                        DqtMqEstablishmentId = oldEstablishment?.Id,
                        DqtMqEstablishmentName = oldEstablishment?.dfeta_name
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
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = newProvider!.MandatoryQualificationProviderId,
                        Name = newProvider.Name,
                        DqtMqEstablishmentId = NewMqEstablishment!.Id,
                        DqtMqEstablishmentName = NewMqEstablishment.dfeta_name
                    }
                },
                OldMandatoryQualification = oldMqEventModel,
                Changes = changes
            };

            await crmQueryDispatcher.ExecuteQuery(
                new UpdateMandatoryQualificationEstablishmentQuery(
                    QualificationId,
                    NewMqEstablishment!.Id,
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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqEditProvider(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentMqEstablishmentName = JourneyInstance!.State.CurrentMqEstablishmentName;
        NewMqEstablishment = await referenceDataCache.GetMqEstablishmentByValue(JourneyInstance!.State.MqEstablishmentValue!);

        await next();
    }
}

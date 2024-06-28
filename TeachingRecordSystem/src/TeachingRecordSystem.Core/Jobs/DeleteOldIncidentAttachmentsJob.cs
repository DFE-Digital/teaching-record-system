using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs;

public class DeleteOldIncidentAttachmentsJob(ICrmQueryDispatcher crmQueryDispatcher, ReferenceDataCache referenceDataCache, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";

    private static readonly TimeSpan _modifiedBeforeWindow = TimeSpan.FromDays(30);

    public async Task Execute(CancellationToken cancellationToken)
    {
        var changeDateOfBirthSubject = await referenceDataCache.GetSubjectByTitle("Change of Date of Birth");
        var changeNameSubject = await referenceDataCache.GetSubjectByTitle("Change of Name");

        var modifiedBefore = clock.UtcNow.Subtract(_modifiedBeforeWindow);

        var annotations = await crmQueryDispatcher.ExecuteQuery(
            new GetResolvedIncidentAnnotationsQuery(SubjectIds: [changeDateOfBirthSubject.Id, changeNameSubject.Id], modifiedBefore, ColumnSet: new()));

        foreach (var annotation in annotations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await crmQueryDispatcher.ExecuteQuery(
                new DeleteAnnotationQuery(
                    annotation.Id,
                    Event: EventInfo.Create(new DqtAnnotationDeletedEvent()
                    {
                        AnnotationId = annotation.Id,
                        CreatedUtc = clock.UtcNow,
                        EventId = Guid.NewGuid(),
                        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                    })));
        }
    }
}

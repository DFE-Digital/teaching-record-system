using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Jobs;

public class DeleteOldAttachmentsJob(ICrmQueryDispatcher crmQueryDispatcher, ReferenceDataCache referenceDataCache, IClock clock)
{
    public const string JobSchedule = "0 3 * * *";

    private static readonly TimeSpan _modifiedBeforeWindow = TimeSpan.FromDays(30);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var changeDateOfBirthSubject = await referenceDataCache.GetSubjectByTitleAsync("Change of Date of Birth");
        var changeNameSubject = await referenceDataCache.GetSubjectByTitleAsync("Change of Name");

        var modifiedBefore = clock.UtcNow.Subtract(_modifiedBeforeWindow);

        var annotationIds = new List<Guid>();

        await foreach (var annotations in crmQueryDispatcher.ExecuteQueryAsync(
            new GetResolvedIncidentAnnotationsQuery(SubjectIds: [changeDateOfBirthSubject.Id, changeNameSubject.Id], modifiedBefore, ColumnSet: new()), cancellationToken))
        {
            annotationIds.AddRange(annotations.Select(i => i.AnnotationId!.Value));
        }

        await foreach (var annotations in crmQueryDispatcher.ExecuteQueryAsync(
            new GetNonOpenTaskAnnotationsQuery(Subjects: [CreateTrnRequestTaskQuery.TaskSubject], modifiedBefore, ColumnSet: new()), cancellationToken))
        {
            annotationIds.AddRange(annotations.Select(i => i.AnnotationId!.Value));
        }

        foreach (var annotationId in annotationIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await crmQueryDispatcher.ExecuteQueryAsync(
                new DeleteAnnotationQuery(
                    annotationId,
                    Event: EventInfo.Create(new DqtAnnotationDeletedEvent()
                    {
                        AnnotationId = annotationId,
                        CreatedUtc = clock.UtcNow,
                        EventId = Guid.NewGuid(),
                        RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
                    })));
        }
    }
}

using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task DeleteMandatoryQualificationAsync(
        Guid qualificationId,
        RaisedByUserInfo deletedBy,
        string? deletionReason = null,
        string? deletionReasonDetail = null,
        (Guid FileId, string Name)? evidenceFile = null)
    {
        return WithDbContextAsync(async dbContext =>
        {
            var now = Clock.UtcNow;

            var qualification = await dbContext.MandatoryQualifications
                .Include(q => q.Provider)
                .SingleAsync(q => q.QualificationId == qualificationId);

            qualification.DeletedOn = now;

            var mqEstablishment = qualification.DqtMqEstablishmentValue is string mqEstablishmentValue ?
                LegacyDataCache.Instance.GetMqEstablishmentByValue(mqEstablishmentValue) :
                null;

            var deletedEvent = new MandatoryQualificationDeletedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = deletedBy ?? Core.DataStore.Postgres.Models.SystemUser.SystemUserId,
                PersonId = qualification.PersonId,
                MandatoryQualification = new()
                {
                    QualificationId = qualification.QualificationId,
                    Provider = qualification.ProviderId is not null || mqEstablishment is not null ?
                        new MandatoryQualificationProvider()
                        {
                            MandatoryQualificationProviderId = qualification.ProviderId,
                            Name = qualification.ProviderId is not null ?
                                qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                                null,
                            DqtMqEstablishmentName = mqEstablishment?.Name,
                            DqtMqEstablishmentValue = mqEstablishment?.Value
                        } :
                        null,
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                DeletionReason = deletionReason ?? "Added in error",
                DeletionReasonDetail = deletionReasonDetail,
                EvidenceFile = evidenceFile is (Guid FileId, string Name) ?
                    new EventModels.File()
                    {
                        FileId = FileId,
                        Name = Name
                    } :
                    null
            };
            dbContext.AddEvent(deletedEvent);

            await dbContext.SaveChangesAsync();
        });
    }
}

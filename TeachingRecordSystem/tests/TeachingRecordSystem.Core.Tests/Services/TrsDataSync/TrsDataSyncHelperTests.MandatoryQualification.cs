using FakeXrmEasy.Extensions;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Models;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SyncMandatoryQualification_NewRecord_WritesNewRowToDb(bool personAlreadySynced)
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(personAlreadySynced));
        var qualificationId = Guid.NewGuid();
        var entity = await CreateMandatoryQualificationEntity(qualificationId, person.ContactId);

        // Act
        await Helper.SyncMandatoryQualification(entity, ignoreInvalid: false);

        // Assert
        await AssertDatabaseMandatoryQualificationMatchesEntity(entity);
    }

    [Fact]
    public async Task SyncMandatoryQualification_ExistingRecord_UpdatesExistingRowInDb()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var existingEntity = await CreateMandatoryQualificationEntity(qualificationId, person.ContactId);

        await Helper.SyncMandatoryQualification(existingEntity, ignoreInvalid: false);
        var expectedFirstSync = Clock.UtcNow;

        Clock.Advance();
        var updatedEntity = await CreateMandatoryQualificationEntity(qualificationId, person.ContactId, existingEntity);

        // Act
        await Helper.SyncMandatoryQualification(updatedEntity, ignoreInvalid: false);

        // Assert
        await AssertDatabaseMandatoryQualificationMatchesEntity(updatedEntity, expectedFirstSync);
    }

    [Fact]
    public async Task DeleteRecords_WithMq_RemovesRowFromDb()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var existingEntity = await CreateMandatoryQualificationEntity(qualificationId, person.ContactId);

        await Helper.SyncMandatoryQualification(existingEntity, ignoreInvalid: false);

        // Act
        await Helper.DeleteRecords(TrsDataSyncHelper.ModelTypes.MandatoryQualification, new[] { qualificationId });

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications.SingleOrDefaultAsync(p => p.DqtQualificationId == qualificationId);
            Assert.Null(mq);
        });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDeletedEvent_SetsDeletedOnPropertyAndSavesEvent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var entity = await CreateMandatoryQualificationEntity(qualificationId, person.ContactId);

        var specialism = (await TestData.ReferenceDataCache.GetMqSpecialisms())
            .Single(s => s.Id == entity.dfeta_MQ_SpecialismId?.Id)
            .ToMandatoryQualificationSpecialism();

        var dqtUserId = await TestData.GetCurrentCrmUserId();

        var deletedEvent = new MandatoryQualificationDeletedEvent()
        {
            CreatedUtc = Clock.UtcNow,
            DeletionReason = "Added in error",
            DeletionReasonDetail = "Some extra information",
            EventId = Guid.NewGuid(),
            EvidenceFile = null,
            MandatoryQualification = new()
            {
                QualificationId = qualificationId,
                Specialism = specialism,
                Status = entity.dfeta_MQ_Status?.ToMandatoryQualificationStatus(),
                EndDate = entity.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                StartDate = entity.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            },
            PersonId = person.PersonId,
            RaisedBy = RaisedByUserInfo.FromDqtUser(dqtUserId, "Test User")
        };
        entity.dfeta_TrsDeletedEvent = EventInfo.Create(deletedEvent).Serialize();
        entity.StateCode = dfeta_qualificationState.Inactive;

        // Act
        await Helper.SyncMandatoryQualification(entity, ignoreInvalid: false);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.DqtQualificationId == qualificationId);
            Assert.NotNull(mq);
            mq.DeletedOn = deletedEvent.CreatedUtc;

            var @event = await dbContext.Events.SingleOrDefaultAsync(e => e.EventId == deletedEvent.EventId);
            Assert.Equivalent(deletedEvent, @event?.ToEventBase());
        });
    }

    private async Task AssertDatabaseMandatoryQualificationMatchesEntity(dfeta_qualification entity, DateTime? expectedFirstSync = null)
    {
        await DbFixture.WithDbContext(async dbContext =>
        {
            var expectedSpecialism = (await TestData.ReferenceDataCache.GetMqSpecialisms())
                .Single(s => s.Id == entity.dfeta_MQ_SpecialismId?.Id)
                .ToMandatoryQualificationSpecialism();

            var mq = await dbContext.MandatoryQualifications.SingleOrDefaultAsync(p => p.DqtQualificationId == entity.Id);
            Assert.NotNull(mq);
            Assert.Equal(entity.Id, mq.QualificationId);
            Assert.Equal(entity.CreatedOn, mq.CreatedOn);
            Assert.Equal(entity.ModifiedOn, mq.UpdatedOn);
            Assert.Null(mq.DeletedOn);
            Assert.Equal(QualificationType.MandatoryQualification, mq.QualificationType);
            Assert.Equal(entity.dfeta_PersonId?.Id, mq.PersonId);
            Assert.Equal(expectedFirstSync ?? Clock.UtcNow, mq.DqtFirstSync);
            Assert.Equal(Clock.UtcNow, mq.DqtLastSync);
            Assert.Equal((int)entity.StateCode!, mq.DqtState);
            Assert.Equal(entity.CreatedOn, mq.DqtCreatedOn);
            Assert.Equal(entity.ModifiedOn, mq.DqtModifiedOn);
            Assert.Equal(expectedSpecialism, mq.Specialism);
            Assert.Equal(entity.dfeta_MQ_Status?.ToMandatoryQualificationStatus(), mq.Status);
            Assert.Equal(entity.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), mq.StartDate);
            Assert.Equal(entity.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true), mq.EndDate);
            Assert.Equal(entity.dfeta_MQ_MQEstablishmentId?.Id, mq.DqtMqEstablishmentId);
            Assert.Equal(entity.dfeta_MQ_SpecialismId?.Id, mq.DqtSpecialismId);
        });
    }

    private async Task<dfeta_qualification> CreateMandatoryQualificationEntity(
        Guid qualificationId,
        Guid personContactId,
        dfeta_qualification? existingQualification = null)
    {
        var specialisms = await TestData.ReferenceDataCache.GetMqSpecialisms();
        var establishments = await TestData.ReferenceDataCache.GetMqEstablishments();

        var modified = Clock.UtcNow;
        var state = dfeta_qualificationState.Active;
        var specialism = specialisms.RandomOne();
        var establishment = establishments.RandomOne();
        var status = dfeta_qualification_dfeta_MQ_Status.Passed;
        var startDate = new DateOnly(2020, 4, 10);
        var endDate = new DateOnly(2020, 10, 1);

        var newQualification = existingQualification?.Clone<dfeta_qualification>() ?? new()
        {
            Id = qualificationId,
            dfeta_qualificationId = qualificationId,
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_PersonId = personContactId.ToEntityReference(Contact.EntityLogicalName),
            CreatedOn = Clock.UtcNow
        };

        newQualification.ModifiedOn = modified;
        newQualification.StateCode = state;
        newQualification.dfeta_MQ_SpecialismId = specialism.dfeta_specialismId!.Value.ToEntityReference(dfeta_specialism.EntityLogicalName);
        newQualification.dfeta_MQ_MQEstablishmentId = establishment.Id.ToEntityReference(dfeta_mqestablishment.EntityLogicalName);
        newQualification.dfeta_MQ_Status = status;
        newQualification.dfeta_MQStartDate = startDate.ToDateTimeWithDqtBstFix(isLocalTime: true);
        newQualification.dfeta_MQ_Date = endDate.ToDateTimeWithDqtBstFix(isLocalTime: true);

        return newQualification;
    }
}

using System.Diagnostics;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
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
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(entity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await AssertDatabaseMandatoryQualificationMatchesEntity(entity, expectMigrationMappingsApplied: true);
    }

    [Fact]
    public async Task SyncMandatoryQualification_ExistingRecord_UpdatesExistingRowInDb()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var existingEntity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        await Helper.SyncMandatoryQualification(existingEntity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);
        var expectedFirstSync = Clock.UtcNow;

        Clock.Advance();
        var updatedVersion = await CreateUpdatedVersionVersion(existingEntity, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await AssertDatabaseMandatoryQualificationMatchesEntity(updatedVersion, expectMigrationMappingsApplied: true, expectedFirstSync);
    }

    [Fact]
    public async Task DeleteRecords_WithMq_RemovesRowFromDb()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var existingEntity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        await Helper.SyncMandatoryQualification(existingEntity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

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
    public async Task SyncMandatoryQualification_AlreadyHaveNewerVersion_DoesNotUpdateDatabase()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var updatedVersion = await CreateUpdatedVersionVersion(initialVersion, auditDetailCollection);

        await Helper.SyncMandatoryQualification(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);
        var expectedFirstSync = Clock.UtcNow;
        var expectedLastSync = Clock.UtcNow;

        // Act
        await Helper.SyncMandatoryQualification(initialVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await AssertDatabaseMandatoryQualificationMatchesEntity(updatedVersion, expectMigrationMappingsApplied: true, expectedFirstSync, expectedLastSync);
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDeletedEvent_SetsDeletedOnAttribute()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var (deletedVersion, deletedEvent) = await CreateDeletedEntityVersion(entity, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(deletedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.DqtQualificationId == qualificationId);
            Assert.NotNull(mq);
            Assert.Equal(deletedEvent.CreatedUtc, mq.DeletedOn);
        });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDeactivatedEvent_SetsDeletedOnAttribute()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersion(entity, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(deactivatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.DqtQualificationId == qualificationId);
            Assert.NotNull(mq);
            Assert.Equal(Clock.UtcNow, mq.DeletedOn);
        });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDqtCreateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection, addCreateAudit: true);

        // Act
        await Helper.SyncMandatoryQualification(initialVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var createdEvent = Assert.IsType<MandatoryQualificationCreatedEvent>(e);
                Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), createdEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, createdEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, createdEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithNoDqtAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection, addCreateAudit: false);

        // Act
        await Helper.SyncMandatoryQualification(initialVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var migatedEvent = Assert.IsType<MandatoryQualificationDqtImportedEvent>(e);
                Assert.Equal(Clock.UtcNow, migatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), migatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, migatedEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, migatedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithNoDqtCreateButWithUpdateAudits_CreatesExpectedEvents()
    {
        // Many migrated records in DQT don't have a 'Create' audit record, since auditing was turned on after migration.
        // In that case, TrsDataSyncHelper will take the current version and 'un-apply' every Update audit in reverse,
        // leaving the initial version at the end. From that a MandatoryQualificationDqtImported event is created.
        // This test is exercising that scenario.
        // The Updates created here are deliberately partial updates to verify that the original version of the entity can
        // be reconstructed from multiple Update audits.

        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection, addCreateAudit: false);
        var created = Clock.UtcNow;

        Clock.Advance();
        var updatedVersion = await CreateUpdatedVersionVersion(initialVersion, auditDetailCollection, changes: MandatoryQualificationUpdatedEventChanges.Specialism);

        Clock.Advance();
        updatedVersion = await CreateUpdatedVersionVersion(updatedVersion, auditDetailCollection, changes: MandatoryQualificationUpdatedEventChanges.Provider);

        // Act
        await Helper.SyncMandatoryQualification(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var importedEvent = Assert.IsType<MandatoryQualificationDqtImportedEvent>(e);
                Assert.Equal(created, importedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), importedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, importedEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, importedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
            },
            e =>
            {
                // Checking UpdatedEvent details is covered elsewhere
                Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                // Checking UpdatedEvent details is covered elsewhere
                Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(updatedVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithTrsEventAttributeOnDqtCreateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var trsEventId = Guid.NewGuid();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection, trsAuditEventId: trsEventId);

        // Act
        await Helper.SyncMandatoryQualification(initialVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var createdEvent = Assert.IsType<MandatoryQualificationCreatedEvent>(e);
                Assert.Equal(trsEventId, createdEvent.EventId);
                Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), createdEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, createdEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, createdEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(initialVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDqtUpdateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var updatedVersion = await CreateUpdatedVersionVersion(initialVersion, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                // Checking CreatedEvent details is covered elsewhere
                Assert.IsType<MandatoryQualificationCreatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var updatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, updatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), updatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, updatedEvent.PersonId);
                await AssertEventMatchesEntity(updatedVersion, updatedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
                Assert.Equal(GetChanges(initialVersion, updatedVersion), updatedEvent.Changes);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(updatedVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithTrsEventAttributeOnDqtUpdateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var trsEventId = Guid.NewGuid();
        var updatedVersion = await CreateUpdatedVersionVersion(initialVersion, auditDetailCollection, trsEventId);

        // Act
        await Helper.SyncMandatoryQualification(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                // Checking CreatedEvent details is covered elsewhere
                Assert.IsType<MandatoryQualificationCreatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var updatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
                Assert.Equal(trsEventId, updatedEvent.EventId);
                Assert.Equal(Clock.UtcNow, updatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), updatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, updatedEvent.PersonId);
                await AssertEventMatchesEntity(updatedVersion, updatedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
                Assert.Equal(GetChanges(initialVersion, updatedVersion), updatedEvent.Changes);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(updatedVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDqtDeactivatedAudit_CreatesExpectedEvent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersion(entity, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(deactivatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);
        Assert.Equal(2, events.Length);
        var lastEvent = events.Last();
        var deactivatedEvent = Assert.IsType<MandatoryQualificationDqtDeactivatedEvent>(lastEvent);
        Assert.Equal(Clock.UtcNow, deactivatedEvent.CreatedUtc);
        Assert.Equal(await TestData.GetCurrentCrmUserId(), deactivatedEvent.RaisedBy.DqtUserId);
        Assert.Equal(person.PersonId, deactivatedEvent.PersonId);
        await AssertEventMatchesEntity(deactivatedVersion, deactivatedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDqtReactivatedAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersion(entity, auditDetailCollection);

        Clock.Advance();
        var reactivatedVersion = await CreateReactivatedEntityVersion(deactivatedVersion, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(reactivatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                Assert.IsType<MandatoryQualificationCreatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                Assert.IsType<MandatoryQualificationDqtDeactivatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var reactivatedEvent = Assert.IsType<MandatoryQualificationDqtReactivatedEvent>(e);
                Assert.Equal(Clock.UtcNow, reactivatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), reactivatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, reactivatedEvent.PersonId);
                await AssertEventMatchesEntity(reactivatedVersion, reactivatedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<MandatoryQualificationMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertEventMatchesEntity(reactivatedVersion, migratedEvent.MandatoryQualification, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncMandatoryQualification_WithDeletedAudit_CreatesExpectedEvent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var (deletedVersion, deletedEvent) = await CreateDeletedEntityVersion(entity, auditDetailCollection);

        // Act
        await Helper.SyncMandatoryQualification(deletedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForQualification(qualificationId);
        Assert.Equal(2, events.Length);
        var lastEvent = events.Last();
        var actualDeletedEvent = Assert.IsType<MandatoryQualificationDeletedEvent>(lastEvent);
        Assert.Equal(deletedEvent.EventId, actualDeletedEvent.EventId);
        Assert.Equal(Clock.UtcNow, actualDeletedEvent.CreatedUtc);
        Assert.Equal(await TestData.GetCurrentCrmUserId(), actualDeletedEvent.RaisedBy.DqtUserId);
        Assert.Equal(person.PersonId, actualDeletedEvent.PersonId);
        await AssertEventMatchesEntity(deletedVersion, actualDeletedEvent.MandatoryQualification, expectMigrationMappingsApplied: false);
    }

    private static MandatoryQualificationUpdatedEventChanges GetChanges(dfeta_qualification first, dfeta_qualification second) =>
        MandatoryQualificationUpdatedEventChanges.None |
        (first.dfeta_MQ_MQEstablishmentId?.Id != second.dfeta_MQ_MQEstablishmentId?.Id ? MandatoryQualificationUpdatedEventChanges.Provider : 0) |
        (first.dfeta_MQ_SpecialismId?.Id != second.dfeta_MQ_SpecialismId?.Id ? MandatoryQualificationUpdatedEventChanges.Specialism : 0) |
        (first.dfeta_MQ_Status != second.dfeta_MQ_Status ? MandatoryQualificationUpdatedEventChanges.Status : 0) |
        (first.dfeta_MQStartDate != second.dfeta_MQStartDate ? MandatoryQualificationUpdatedEventChanges.StartDate : 0) |
        (first.dfeta_MQ_Date != second.dfeta_MQ_Date ? MandatoryQualificationUpdatedEventChanges.EndDate : 0);

    private async Task AssertDatabaseMandatoryQualificationMatchesEntity(
        dfeta_qualification entity,
        bool expectMigrationMappingsApplied,
        DateTime? expectedFirstSync = null,
        DateTime? expectedLastSync = null)
    {
        await DbFixture.WithDbContext(async dbContext =>
        {
            var mqEstablishment = entity.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
                await TestData.ReferenceDataCache.GetMqEstablishmentById(establishmentId) :
                null;

            Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(mqEstablishment, out var expectedProvider);

            var mqSpecialism = entity.dfeta_MQ_SpecialismId?.Id is Guid dqtSpecialismId ?
                (await TestData.ReferenceDataCache.GetMqSpecialismById(dqtSpecialismId)) :
                (dfeta_specialism?)null;

            var mq = await dbContext.MandatoryQualifications.SingleOrDefaultAsync(p => p.DqtQualificationId == entity.Id);
            Assert.NotNull(mq);
            Assert.Equal(entity.Id, mq.QualificationId);
            Assert.Equal(entity.CreatedOn, mq.CreatedOn);
            Assert.Equal(entity.ModifiedOn, mq.UpdatedOn);
            Assert.Null(mq.DeletedOn);
            Assert.Equal(QualificationType.MandatoryQualification, mq.QualificationType);
            Assert.Equal(entity.dfeta_PersonId?.Id, mq.PersonId);
            Assert.Equal(expectedFirstSync ?? Clock.UtcNow, mq.DqtFirstSync);
            Assert.Equal(expectedLastSync ?? Clock.UtcNow, mq.DqtLastSync);
            Assert.Equal((int)entity.StateCode!, mq.DqtState);
            Assert.Equal(entity.CreatedOn, mq.DqtCreatedOn);
            Assert.Equal(entity.ModifiedOn, mq.DqtModifiedOn);
            Assert.Equal(expectedProvider?.MandatoryQualificationProviderId, mq.ProviderId);
            //Assert.Equal(expectedSpecialism, mq.Specialism);
            Assert.Equal(entity.dfeta_MQ_Status?.ToMandatoryQualificationStatus(), mq.Status);
            Assert.Equal(entity.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), mq.StartDate);
            Assert.Equal(entity.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true), mq.EndDate);
            Assert.Equal(entity.dfeta_MQ_MQEstablishmentId?.Id, mq.DqtMqEstablishmentId);
            Assert.Equal(entity.dfeta_MQ_SpecialismId?.Id, mq.DqtSpecialismId);

            if (expectMigrationMappingsApplied && mqEstablishment is not null && mqSpecialism is not null)
            {
                MandatoryQualificationSpecialismRegistry.TryMapFromDqtMqEstablishment(mqEstablishment.dfeta_Value, mqSpecialism.dfeta_Value, out var expectedSpecialism);
                expectedSpecialism ??= mqSpecialism?.ToMandatoryQualificationSpecialism();
                Assert.Equal(expectedSpecialism, mq.Specialism);
            }
            else
            {
                Assert.Equal(mqSpecialism?.ToMandatoryQualificationSpecialism(), mq.Specialism);
            }
        });
    }

    private async Task AssertEventMatchesEntity(
        dfeta_qualification entity,
        EventModels.MandatoryQualification eventModel,
        bool expectMigrationMappingsApplied)
    {
        var mqEstablishment = entity.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
            await TestData.ReferenceDataCache.GetMqEstablishmentById(establishmentId) :
            null;

        var mqSpecialism = entity.dfeta_MQ_SpecialismId?.Id is Guid dqtSpecialismId ?
            (await TestData.ReferenceDataCache.GetMqSpecialismById(dqtSpecialismId)) :
            (dfeta_specialism?)null;

        var status = entity.dfeta_MQ_Status is dfeta_qualification_dfeta_MQ_Status dqtStatus ?
            dqtStatus.ToMandatoryQualificationStatus() :
            (MandatoryQualificationStatus?)null;

        Assert.Equal(entity.Id, eventModel.QualificationId);
        Assert.Equal(mqEstablishment?.Id, eventModel.Provider?.DqtMqEstablishmentId);
        Assert.Equal(mqEstablishment?.dfeta_name, eventModel.Provider?.DqtMqEstablishmentName);
        Assert.Equal(status, eventModel.Status);
        Assert.Equal(entity.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), eventModel.StartDate);
        Assert.Equal(entity.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true), eventModel.EndDate);

        if (expectMigrationMappingsApplied && mqEstablishment is not null)
        {
            Core.DataStore.Postgres.Models.MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(mqEstablishment, out var expectedProvider);
            Assert.NotNull(expectedProvider);

            Assert.Equal(expectedProvider.MandatoryQualificationProviderId, eventModel.Provider?.MandatoryQualificationProviderId);
            Assert.Equal(expectedProvider.Name, eventModel.Provider?.Name);
        }
        else
        {
            Assert.False(eventModel.Provider?.MandatoryQualificationProviderId.HasValue);
            Assert.Null(eventModel.Provider?.Name);
        }

        if (expectMigrationMappingsApplied && mqEstablishment is not null && mqSpecialism is not null)
        {
            MandatoryQualificationSpecialismRegistry.TryMapFromDqtMqEstablishment(mqEstablishment.dfeta_Value, mqSpecialism.dfeta_Value, out var expectedSpecialism);
            expectedSpecialism ??= mqSpecialism?.ToMandatoryQualificationSpecialism();
            Assert.Equal(expectedSpecialism, eventModel.Specialism);
        }
        else
        {
            Assert.Equal(mqSpecialism?.ToMandatoryQualificationSpecialism(), eventModel.Specialism);
        }
    }

    private async Task<dfeta_qualification> CreateNewEntityVersion(
        Guid qualificationId,
        Guid personContactId,
        AuditDetailCollection auditDetailCollection,
        bool addCreateAudit = true,
        Guid? trsAuditEventId = null)
    {
        if (addCreateAudit == false && trsAuditEventId.HasValue)
        {
            throw new ArgumentException("Cannot add a TRS Event without the create audit.");
        }

        Debug.Assert(auditDetailCollection.Count == 0);

        var specialisms = await TestData.ReferenceDataCache.GetMqSpecialisms();
        var establishments = await TestData.ReferenceDataCache.GetMqEstablishments();
        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.UtcNow;
        var state = dfeta_qualificationState.Active;

        var specialism = specialisms.RandomOne();
        var establishment = establishments.RandomOne();
        var status = Enum.GetValues<dfeta_qualification_dfeta_MQ_Status>().RandomOne();
        var startDate = new DateOnly(2020, 4, Random.Shared.Next(1, 30)).ToDateTimeWithDqtBstFix(isLocalTime: true);
        var endDate = status == dfeta_qualification_dfeta_MQ_Status.Passed ? (DateTime?)startDate.AddMonths(6) : null;

        var newQualification = new dfeta_qualification()
        {
            Id = qualificationId,
            dfeta_qualificationId = qualificationId,
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_PersonId = personContactId.ToEntityReference(Contact.EntityLogicalName),
            CreatedOn = createdOn,
            CreatedBy = currentDqtUser,
            ModifiedOn = modifiedOn,
            StateCode = state,
            dfeta_MQ_SpecialismId = specialism.dfeta_specialismId!.Value.ToEntityReference(dfeta_specialism.EntityLogicalName),
            dfeta_MQ_MQEstablishmentId = establishment.Id.ToEntityReference(dfeta_mqestablishment.EntityLogicalName),
            dfeta_MQ_Status = status,
            dfeta_MQStartDate = startDate,
            dfeta_MQ_Date = endDate
        };

        if (trsAuditEventId is Guid eventId)
        {
            var updatedEvent = new MandatoryQualificationCreatedEvent()
            {
                EventId = eventId,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(currentDqtUser.Id, currentDqtUser.Name),
                PersonId = newQualification.dfeta_PersonId.Id,
                MandatoryQualification = new()
                {
                    QualificationId = newQualification.Id,
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
                    Status = status.ToMandatoryQualificationStatus(),
                    StartDate = startDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    EndDate = endDate.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                }
            };

            var serializedEvent = EventInfo.Create(updatedEvent).Serialize();
            newQualification.dfeta_TRSEvent = serializedEvent;
        }

        if (addCreateAudit)
        {
            var auditId = Guid.NewGuid();
            auditDetailCollection.Add(new AttributeAuditDetail()
            {
                AuditRecord = new Audit()
                {
                    Action = Audit_Action.Create,
                    AuditId = auditId,
                    CreatedOn = Clock.UtcNow,
                    Id = auditId,
                    Operation = Audit_Operation.Create,
                    UserId = currentDqtUser
                },
                OldValue = new Entity(),
                NewValue = newQualification.Clone()
            });
        }

        return newQualification;
    }

    private async Task<dfeta_qualification> CreateUpdatedVersionVersion(
        dfeta_qualification existingQualification,
        AuditDetailCollection auditDetailCollection,
        Guid? trsAuditEventId = null,
        MandatoryQualificationUpdatedEventChanges? changes = null)
    {
        if (changes == MandatoryQualificationUpdatedEventChanges.None)
        {
            throw new ArgumentException("Changes cannot be None.", nameof(changes));
        }

        bool ChangeRequested(MandatoryQualificationUpdatedEventChanges field) =>
            changes is null || changes.Value.HasFlag(field);

        var specialisms = await TestData.ReferenceDataCache.GetMqSpecialisms();
        var establishments = await TestData.ReferenceDataCache.GetMqEstablishments();
        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var existingSpecialism = existingQualification.dfeta_MQ_SpecialismId?.Id is Guid specialismId ?
            await TestData.ReferenceDataCache.GetMqSpecialismById(specialismId) :
            null;

        var specialism = ChangeRequested(MandatoryQualificationUpdatedEventChanges.Specialism) ?
            specialisms.RandomOneExcept(s => s.Id == existingQualification.dfeta_MQ_SpecialismId?.Id) :
            existingSpecialism;

        var existingEstablishment = existingQualification.dfeta_MQ_MQEstablishmentId?.Id is Guid establishmentId ?
            await TestData.ReferenceDataCache.GetMqEstablishmentById(establishmentId) :
            null;

        var establishment = ChangeRequested(MandatoryQualificationUpdatedEventChanges.Provider) ?
            establishments.RandomOneExcept(e => e.Id == existingQualification.dfeta_MQ_MQEstablishmentId?.Id) :
            existingEstablishment;

        var existingStatus = existingQualification.dfeta_MQ_Status;

        var status = ChangeRequested(MandatoryQualificationUpdatedEventChanges.Status) ?
            Enum.GetValues<dfeta_qualification_dfeta_MQ_Status>().RandomOneExcept(s => s == existingQualification.dfeta_MQ_Status) :
            existingStatus;

        var existingStartDate = existingQualification.dfeta_MQStartDate;

        var startDate = ChangeRequested(MandatoryQualificationUpdatedEventChanges.StartDate) ?
            (existingQualification.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true) is DateOnly existingStartDateOnly ?
                TestData.GenerateChangedDate(existingStartDateOnly, min: new DateOnly(2020, 4, 1)) :
                TestData.GenerateDate(min: new DateOnly(2020, 4, 1))).ToDateTimeWithDqtBstFix(isLocalTime: true) :
            existingStartDate;

        var existingEndDate = existingQualification.dfeta_MQ_Date;

        var endDate = ChangeRequested(MandatoryQualificationUpdatedEventChanges.EndDate) ?
            (status == dfeta_qualification_dfeta_MQ_Status.Passed ? startDate?.AddMonths(6) : null) :
            existingEndDate;

        var updatedQualification = existingQualification.Clone<dfeta_qualification>();
        updatedQualification.ModifiedOn = Clock.UtcNow;
        updatedQualification.dfeta_MQ_SpecialismId = specialism?.dfeta_specialismId?.ToEntityReference(dfeta_specialism.EntityLogicalName);
        updatedQualification.dfeta_MQ_MQEstablishmentId = establishment?.Id.ToEntityReference(dfeta_mqestablishment.EntityLogicalName);
        updatedQualification.dfeta_MQ_Status = status;
        updatedQualification.dfeta_MQStartDate = startDate;
        updatedQualification.dfeta_MQ_Date = endDate;

        var changedAttrs = (
            from newAttr in updatedQualification.Attributes
            join oldAttr in existingQualification.Attributes on newAttr.Key equals oldAttr.Key
            where !AttributeValuesEqual(newAttr.Value, oldAttr.Value)
            select newAttr.Key).ToArray();

        var oldValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        Array.ForEach(changedAttrs, a => oldValue.Attributes[a] = existingQualification.Attributes[a]);

        var newValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        Array.ForEach(changedAttrs, a => newValue.Attributes[a] = updatedQualification.Attributes[a]);

        if (trsAuditEventId is Guid eventId)
        {
            var updatedEvent = new MandatoryQualificationUpdatedEvent()
            {
                EventId = eventId,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(currentDqtUser.Id, currentDqtUser.Name),
                PersonId = updatedQualification.dfeta_PersonId.Id,
                MandatoryQualification = new()
                {
                    QualificationId = updatedQualification.Id,
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
                    Status = status?.ToMandatoryQualificationStatus(),
                    StartDate = startDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    EndDate = endDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                },
                OldMandatoryQualification = new()
                {
                    QualificationId = existingQualification.Id,
                    Provider = establishment is not null ?
                        new()
                        {
                            MandatoryQualificationProviderId = null,
                            Name = null,
                            DqtMqEstablishmentId = existingEstablishment?.Id,
                            DqtMqEstablishmentName = existingEstablishment?.dfeta_name
                        } :
                        null,
                    Specialism = existingSpecialism?.ToMandatoryQualificationSpecialism(),
                    Status = existingStatus?.ToMandatoryQualificationStatus(),
                    StartDate = existingStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    EndDate = existingEndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                },
                ChangeReason = null,
                ChangeReasonDetail = null,
                EvidenceFile = null,
                Changes = GetChanges(existingQualification, updatedQualification)
            };

            var serializedEvent = EventInfo.Create(updatedEvent).Serialize();
            updatedQualification.dfeta_TRSEvent = serializedEvent;
            newValue.Attributes[dfeta_qualification.Fields.dfeta_TRSEvent] = serializedEvent;
        }

        var auditId = Guid.NewGuid();
        auditDetailCollection.Add(new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Update,
                AuditId = auditId,
                CreatedOn = Clock.UtcNow,
                Id = auditId,
                Operation = Audit_Operation.Update,
                UserId = currentDqtUser
            },
            OldValue = oldValue,
            NewValue = newValue
        });

        return updatedQualification;

        static bool AttributeValuesEqual(object? a, object? b) =>
            a is null && b is null ||
            (a is not null && b is not null && (
                (a is EntityReference aRef && b is EntityReference bRef && aRef.LogicalName == bRef.LogicalName && aRef.Id == bRef.Id) ||
                a.Equals(b)));
    }

    private async Task<(dfeta_qualification Entity, MandatoryQualificationDeletedEvent DeletedEvent)> CreateDeletedEntityVersion(
        dfeta_qualification existingQualification,
        AuditDetailCollection auditDetailCollection)
    {
        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var establishment = await TestData.ReferenceDataCache.GetMqEstablishmentById(existingQualification.dfeta_MQ_MQEstablishmentId.Id);

        var specialism = (await TestData.ReferenceDataCache.GetMqSpecialismById(existingQualification.dfeta_MQ_SpecialismId.Id))
            .ToMandatoryQualificationSpecialism();

        var updatedQualification = existingQualification.Clone<dfeta_qualification>();

        var deletedEvent = new MandatoryQualificationDeletedEvent()
        {
            CreatedUtc = Clock.UtcNow,
            DeletionReason = "Added in error",
            DeletionReasonDetail = "Some extra information",
            EventId = Guid.NewGuid(),
            EvidenceFile = null,
            MandatoryQualification = new()
            {
                QualificationId = existingQualification.Id,
                Provider = new()
                {
                    MandatoryQualificationProviderId = null,
                    Name = null,
                    DqtMqEstablishmentId = establishment.Id,
                    DqtMqEstablishmentName = establishment.dfeta_name
                },
                Specialism = specialism,
                Status = existingQualification.dfeta_MQ_Status?.ToMandatoryQualificationStatus(),
                EndDate = existingQualification.dfeta_MQ_Date?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                StartDate = existingQualification.dfeta_MQStartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            },
            PersonId = existingQualification.dfeta_PersonId.Id,
            RaisedBy = EventModels.RaisedByUserInfo.FromDqtUser(currentDqtUser.Id, currentDqtUser.Name)
        };

        updatedQualification.dfeta_TRSEvent = EventInfo.Create(deletedEvent).Serialize();
        updatedQualification.StateCode = dfeta_qualificationState.Inactive;

        var oldValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);

        var newValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        newValue.Attributes[dfeta_qualification.Fields.dfeta_TRSEvent] = updatedQualification.dfeta_TRSEvent;

        var auditId = Guid.NewGuid();
        auditDetailCollection.Add(new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Update,
                AuditId = auditId,
                CreatedOn = Clock.UtcNow,
                Id = auditId,
                Operation = Audit_Operation.Update,
                UserId = currentDqtUser
            },
            OldValue = oldValue,
            NewValue = newValue
        });

        return (updatedQualification, deletedEvent);
    }

    private async Task<dfeta_qualification> CreateDeactivatedEntityVersion(
        dfeta_qualification existingQualification,
        AuditDetailCollection auditDetailCollection)
    {
        if (existingQualification.StateCode != dfeta_qualificationState.Active)
        {
            throw new ArgumentException("Entity must be active.", nameof(existingQualification));
        }

        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var updatedQualification = existingQualification.Clone<dfeta_qualification>();
        updatedQualification.StateCode = dfeta_qualificationState.Inactive;
        updatedQualification.Attributes["statuscode"] = new OptionSetValue(2);

        var oldValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        oldValue.Attributes[dfeta_qualification.Fields.StateCode] = new OptionSetValue((int)dfeta_qualificationState.Active);
        oldValue.Attributes["statuscode"] = new OptionSetValue(1);

        var newValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        newValue.Attributes[dfeta_qualification.Fields.StateCode] = new OptionSetValue((int)dfeta_qualificationState.Inactive);
        newValue.Attributes["statuscode"] = new OptionSetValue(2);

        var auditId = Guid.NewGuid();
        auditDetailCollection.Add(new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Update,
                AuditId = auditId,
                CreatedOn = Clock.UtcNow,
                Id = auditId,
                Operation = Audit_Operation.Update,
                UserId = currentDqtUser
            },
            OldValue = oldValue,
            NewValue = newValue
        });

        return updatedQualification;
    }

    private async Task<dfeta_qualification> CreateReactivatedEntityVersion(
        dfeta_qualification existingQualification,
        AuditDetailCollection auditDetailCollection)
    {
        if (existingQualification.StateCode != dfeta_qualificationState.Inactive)
        {
            throw new ArgumentException("Entity must be inactive.", nameof(existingQualification));
        }

        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var updatedQualification = existingQualification.Clone<dfeta_qualification>();
        updatedQualification.StateCode = dfeta_qualificationState.Active;
        updatedQualification.Attributes["statuscode"] = new OptionSetValue(1);

        var oldValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        oldValue.Attributes[dfeta_qualification.Fields.StateCode] = new OptionSetValue((int)dfeta_qualificationState.Inactive);
        oldValue.Attributes["statuscode"] = new OptionSetValue(2);

        var newValue = new Entity(dfeta_qualification.EntityLogicalName, existingQualification.Id);
        newValue.Attributes[dfeta_qualification.Fields.StateCode] = new OptionSetValue((int)dfeta_qualificationState.Active);
        newValue.Attributes["statuscode"] = new OptionSetValue(1);

        var auditId = Guid.NewGuid();
        auditDetailCollection.Add(new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Update,
                AuditId = auditId,
                CreatedOn = Clock.UtcNow,
                Id = auditId,
                Operation = Audit_Operation.Update,
                UserId = currentDqtUser
            },
            OldValue = oldValue,
            NewValue = newValue
        });

        return updatedQualification;
    }

    private Task<EventBase[]> GetEventsForQualification(Guid qualificationId) =>
        DbFixture.WithDbContext(async dbContext =>
        {
            var results = await dbContext.Database.SqlQuery<EventQueryResult>(
                $"""
                SELECT e.event_name, e.payload
                FROM events as e
                WHERE (e.payload -> 'MandatoryQualification' ->> 'QualificationId')::uuid = {qualificationId}
                ORDER BY e.created
                """).ToArrayAsync();

            return results.Select(r => EventBase.Deserialize(r.Payload, r.EventName)).ToArray();
        });

    private class EventQueryResult
    {
        public required string EventName { get; set; }
        public required string Payload { get; set; }
    }
}

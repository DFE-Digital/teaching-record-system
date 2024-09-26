using System.Diagnostics;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Core.Tests.Services.TrsDataSync;

public partial class TrsDataSyncHelperTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SyncAlert_NewRecord_WritesNewRowToDb(bool personAlreadySynced)
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(personAlreadySynced));
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection);

        // Act
        await Helper.SyncAlert(entity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await AssertDatabaseAlertMatchesEntity(entity);
    }

    [Fact]
    public async Task SyncAlert_SanctionCodeIsRedundant_IsNotWrittenToDb()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection, redundantType: true);

        // Act
        await Helper.SyncAlert(entity, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var alert = await dbContext.Alerts.SingleOrDefaultAsync(p => p.DqtSanctionId == entity.Id);
            Assert.Null(alert);
        });
    }

    [Fact]
    public async Task SyncAlert_WithDeactivatedEvent_SetsDeletedOnAttribute()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithSyncOverride(false));
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(qualificationId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersion(entity, dfeta_sanction.EntityLogicalName, auditDetailCollection);

        // Act
        await Helper.SyncAlert(deactivatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var mq = await dbContext.Alerts.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.DqtSanctionId == qualificationId);
            Assert.NotNull(mq);
            Assert.Equal(Clock.UtcNow, mq.DeletedOn);
        });
    }

    [Fact]
    public async Task SyncAlert_WithDqtCreateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var qualificationId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewAlertEntityVersion(qualificationId, person.ContactId, auditDetailCollection, addCreateAudit: true);

        // Act
        await Helper.SyncAlert(initialVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForAlert(qualificationId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var createdEvent = Assert.IsType<AlertCreatedEvent>(e);
                Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), createdEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, createdEvent.PersonId);
                await AssertAlertEventMatchesEntity(initialVersion, createdEvent.Alert, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<AlertMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertAlertEventMatchesEntity(initialVersion, migratedEvent.Alert, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncAlert_WithNoDqtAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection, addCreateAudit: false);

        // Act
        await Helper.SyncAlert(initialVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForAlert(alertId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var migatedEvent = Assert.IsType<AlertDqtImportedEvent>(e);
                Assert.Equal(Clock.UtcNow, migatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), migatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, migatedEvent.PersonId);
                await AssertAlertEventMatchesEntity(initialVersion, migatedEvent.Alert, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<AlertMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertAlertEventMatchesEntity(initialVersion, migratedEvent.Alert, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncAlert_WithNoDqtCreateButWithUpdateAudits_CreatesExpectedEvents()
    {
        // Many migrated records in DQT don't have a 'Create' audit record, since auditing was turned on after migration.
        // In that case, TrsDataSyncHelper will take the current version and 'un-apply' every Update audit in reverse,
        // leaving the initial version at the end. From that an AlertDqtImported event is created.
        // This test is exercising that scenario.
        // The Updates created here are deliberately partial updates to verify that the original version of the entity can
        // be reconstructed from multiple Update audits.

        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection, addCreateAudit: false);
        var created = Clock.UtcNow;

        Clock.Advance();
        var updatedVersion = await CreateUpdatedAlertEntityVersion(initialVersion, auditDetailCollection, changes: AlertUpdatedEventChanges.StartDate);

        Clock.Advance();
        updatedVersion = await CreateUpdatedAlertEntityVersion(updatedVersion, auditDetailCollection, changes: AlertUpdatedEventChanges.Details);

        // Act
        await Helper.SyncAlert(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForAlert(alertId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var importedEvent = Assert.IsType<AlertDqtImportedEvent>(e);
                Assert.Equal(created, importedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), importedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, importedEvent.PersonId);
                await AssertAlertEventMatchesEntity(initialVersion, importedEvent.Alert, expectMigrationMappingsApplied: false);
            },
            e =>
            {
                // Checking UpdatedEvent details is covered elsewhere
                Assert.IsType<AlertUpdatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                // Checking UpdatedEvent details is covered elsewhere
                Assert.IsType<AlertUpdatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<AlertMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertAlertEventMatchesEntity(updatedVersion, migratedEvent.Alert, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncAlert_WithDqtUpdateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var initialVersion = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var updatedVersion = await CreateUpdatedAlertEntityVersion(initialVersion, auditDetailCollection);

        // Act
        await Helper.SyncAlert(updatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForAlert(alertId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                // Checking CreatedEvent details is covered elsewhere
                Assert.IsType<AlertCreatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var updatedEvent = Assert.IsType<AlertUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, updatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), updatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, updatedEvent.PersonId);
                await AssertAlertEventMatchesEntity(updatedVersion, updatedEvent.Alert, expectMigrationMappingsApplied: false);
                Assert.Equal(GetChanges(initialVersion, updatedVersion), updatedEvent.Changes);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<AlertMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertAlertEventMatchesEntity(updatedVersion, migratedEvent.Alert, expectMigrationMappingsApplied: true);
            });
    }

    [Fact]
    public async Task SyncAlert_WithDqtDeactivatedAudit_CreatesExpectedEvent()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersion(entity, dfeta_sanction.EntityLogicalName, auditDetailCollection);

        // Act
        await Helper.SyncAlert(deactivatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        await DbFixture.WithDbContext(async dbContext =>
        {
            var alert = await dbContext.Alerts.IgnoreQueryFilters().SingleOrDefaultAsync(p => p.DqtSanctionId == alertId);
            Assert.NotNull(alert);
            Assert.Equal(Clock.UtcNow, alert.DeletedOn);
        });
    }

    [Fact]
    public async Task SyncAlert_WithDqtReactivatedAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var auditDetailCollection = new AuditDetailCollection();
        var entity = await CreateNewAlertEntityVersion(alertId, person.ContactId, auditDetailCollection);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersion(entity, dfeta_sanction.EntityLogicalName, auditDetailCollection);

        Clock.Advance();
        var reactivatedVersion = await CreateReactivatedEntityVersion(deactivatedVersion, dfeta_sanction.EntityLogicalName, auditDetailCollection);

        // Act
        await Helper.SyncAlert(reactivatedVersion, auditDetailCollection, ignoreInvalid: false, createMigratedEvent: true);

        // Assert
        var events = await GetEventsForAlert(alertId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                Assert.IsType<AlertCreatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                Assert.IsType<AlertDqtDeactivatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var reactivatedEvent = Assert.IsType<AlertDqtReactivatedEvent>(e);
                Assert.Equal(Clock.UtcNow, reactivatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserId(), reactivatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, reactivatedEvent.PersonId);
                await AssertAlertEventMatchesEntity(reactivatedVersion, reactivatedEvent.Alert, expectMigrationMappingsApplied: false);
            },
            async e =>
            {
                var migratedEvent = Assert.IsType<AlertMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                await AssertAlertEventMatchesEntity(reactivatedVersion, migratedEvent.Alert, expectMigrationMappingsApplied: true);
            });
    }

    private async Task<dfeta_sanction> CreateNewAlertEntityVersion(
        Guid sanctionId,
        Guid personContactId,
        AuditDetailCollection auditDetailCollection,
        bool redundantType = false,
        bool addCreateAudit = true)
    {
        Debug.Assert(auditDetailCollection.Count == 0);

        var sanctionCodes = await TestData.ReferenceDataCache.GetSanctionCodes(activeOnly: false);
        var alertTypes = await TestData.ReferenceDataCache.GetAlertTypes();
        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.UtcNow;
        var state = dfeta_sanctionState.Active;

        // Redundant sanction types won't have a corresponding AlertType (and shouldn't be migrated)
        var sanctionCodeId = sanctionCodes
            .Where(sc => (!redundantType && alertTypes.Any(t => t.DqtSanctionCode == sc.dfeta_Value)) ||
                (redundantType && !alertTypes.Any(t => t.DqtSanctionCode == sc.dfeta_Value)))
            .RandomOne().Id;

        var details = Faker.Lorem.Paragraph();
        var externalLink = Faker.Internet.Url();
        var startDate = new DateOnly(2020, 4, Random.Shared.Next(1, 30)).ToDateTimeWithDqtBstFix(isLocalTime: true);
        var endDate = new[] { true, false }.RandomOne() ? (DateTime?)startDate.AddMonths(6) : null;
        var spent = endDate.HasValue;

        var newSanction = new dfeta_sanction()
        {
            Id = sanctionId,
            dfeta_sanctionId = sanctionId,
            dfeta_PersonId = personContactId.ToEntityReference(Contact.EntityLogicalName),
            CreatedOn = createdOn,
            CreatedBy = currentDqtUser,
            ModifiedOn = modifiedOn,
            StateCode = state,
            dfeta_DetailsLink = externalLink,
            dfeta_SanctionDetails = details,
            dfeta_StartDate = startDate,
            dfeta_EndDate = endDate,
            dfeta_Spent = spent,
            dfeta_SanctionCodeId = sanctionCodeId.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
        };

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
                NewValue = newSanction.Clone()
            });
        }

        return newSanction;
    }

    private async Task<dfeta_sanction> CreateUpdatedAlertEntityVersion(
        dfeta_sanction existingSanction,
        AuditDetailCollection auditDetailCollection,
        AlertUpdatedEventChanges? changes = null)
    {
        if (changes == AlertUpdatedEventChanges.None)
        {
            throw new ArgumentException("Changes cannot be None.", nameof(changes));
        }

        bool ChangeRequested(AlertUpdatedEventChanges field) =>
            changes is null || changes.Value.HasFlag(field);

        var sanctionCodes = await TestData.ReferenceDataCache.GetSanctionCodes(activeOnly: false);
        var currentDqtUser = await TestData.GetCurrentCrmUser();

        var existingExternalLink = existingSanction.dfeta_DetailsLink;

        var externalLink = ChangeRequested(AlertUpdatedEventChanges.ExternalLink) ?
            GenerateChanged(existingExternalLink, Faker.Internet.Url) :
            existingExternalLink;

        var existingDetails = existingSanction.dfeta_SanctionDetails;

        var details = ChangeRequested(AlertUpdatedEventChanges.Details) ?
            GenerateChanged(existingDetails, Faker.Lorem.Paragraph) :
            existingDetails;

        var existingStartDate = existingSanction.dfeta_StartDate;

        var startDate = ChangeRequested(AlertUpdatedEventChanges.StartDate) ?
            (existingSanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true) is DateOnly existingStartDateOnly ?
                TestData.GenerateChangedDate(existingStartDateOnly, min: new DateOnly(2020, 4, 1)) :
                TestData.GenerateDate(min: new DateOnly(2020, 4, 1))).ToDateTimeWithDqtBstFix(isLocalTime: true) :
            existingStartDate;

        var existingEndDate = existingSanction.dfeta_EndDate;
        DateTime? endDate;

        if (ChangeRequested(AlertUpdatedEventChanges.EndDate))
        {
            if (startDate is null)
            {
                throw new InvalidOperationException("Cannot generate an end date when there is no start date.");
            }

            var startDateOnly = startDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true);

            endDate = (existingEndDate is null ?
                    TestData.GenerateDate(min: startDateOnly.AddDays(1)) :
                    TestData.GenerateChangedDate(existingEndDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true), min: startDateOnly.AddDays(1)))
                .ToDateTimeWithDqtBstFix(isLocalTime: true);
        }
        else
        {
            endDate = null;
        }

        if (changes is not null && changes.Value.HasFlag(AlertUpdatedEventChanges.DqtSpent))
        {
            throw new NotSupportedException();
        }

        var spent = existingSanction.dfeta_Spent;

        var existingSanctionCodeId = existingSanction.dfeta_SanctionCodeId.Id;

        var sanctionCodeId = ChangeRequested(AlertUpdatedEventChanges.DqtSanctionCode) ?
            sanctionCodes.Select(sc => sc.Id).RandomOneExcept(id => id == existingSanctionCodeId) :
            existingSanctionCodeId;

        var updatedSanction = existingSanction.Clone<dfeta_sanction>();
        updatedSanction.ModifiedOn = Clock.UtcNow;
        updatedSanction.dfeta_DetailsLink = externalLink;
        updatedSanction.dfeta_SanctionDetails = details;
        updatedSanction.dfeta_StartDate = startDate;
        updatedSanction.dfeta_EndDate = endDate;
        updatedSanction.dfeta_Spent = spent;
        updatedSanction.dfeta_SanctionCodeId = sanctionCodeId.ToEntityReference(dfeta_sanctioncode.EntityLogicalName);

        var changedAttrs = (
            from newAttr in updatedSanction.Attributes
            join oldAttr in existingSanction.Attributes on newAttr.Key equals oldAttr.Key
            where !AttributeValuesEqual(newAttr.Value, oldAttr.Value)
            select newAttr.Key).ToArray();

        var oldValue = new Entity(dfeta_sanction.EntityLogicalName, existingSanction.Id);
        Array.ForEach(changedAttrs, a => oldValue.Attributes[a] = existingSanction.Attributes[a]);

        var newValue = new Entity(dfeta_sanction.EntityLogicalName, existingSanction.Id);
        Array.ForEach(changedAttrs, a => newValue.Attributes[a] = updatedSanction.Attributes[a]);

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

        return updatedSanction;

        static bool AttributeValuesEqual(object? a, object? b) =>
            a is null && b is null ||
            (a is not null && b is not null && a.Equals(b));

        static T? GenerateChanged<T>(T? current, Func<T> factory)
        {
            var value = current;

            while ((value is null && current is null) || (value?.Equals(current) ?? false))
            {
                value = factory();
            }

            return value;
        }
    }

    private static AlertUpdatedEventChanges GetChanges(dfeta_sanction first, dfeta_sanction second) =>
        AlertUpdatedEventChanges.None |
        (first.dfeta_SanctionDetails != second.dfeta_SanctionDetails ? AlertUpdatedEventChanges.Details : 0) |
        (first.dfeta_DetailsLink != second.dfeta_DetailsLink ? AlertUpdatedEventChanges.ExternalLink : 0) |
        (first.dfeta_StartDate != second.dfeta_StartDate ? AlertUpdatedEventChanges.StartDate : 0) |
        (first.dfeta_EndDate != second.dfeta_EndDate ? AlertUpdatedEventChanges.EndDate : 0) |
        (first.dfeta_SanctionCodeId?.Id != second.dfeta_SanctionCodeId?.Id ? AlertUpdatedEventChanges.DqtSanctionCode : 0) |
        (first.dfeta_Spent != second.dfeta_Spent ? AlertUpdatedEventChanges.DqtSpent : 0);

    private async Task AssertDatabaseAlertMatchesEntity(dfeta_sanction entity)
    {
        await DbFixture.WithDbContext(async dbContext =>
        {
            var sanctionCode = await TestData.ReferenceDataCache.GetSanctionCodeById(entity.dfeta_SanctionCodeId.Id);
            var alertTypes = await TestData.ReferenceDataCache.GetAlertTypes();
            var expectedAlertType = alertTypes.Single(t => t.DqtSanctionCode == sanctionCode.dfeta_Value);

            var alert = await dbContext.Alerts.SingleOrDefaultAsync(p => p.DqtSanctionId == entity.Id);
            Assert.NotNull(alert);
            Assert.Equal(entity.Id, alert.AlertId);
            Assert.Equal(entity.CreatedOn, alert.CreatedOn);
            Assert.Equal(entity.ModifiedOn, alert.UpdatedOn);
            Assert.Null(alert.DeletedOn);
            Assert.Equal(entity.dfeta_PersonId?.Id, alert.PersonId);
            Assert.Equal((int)entity.StateCode!, alert.DqtState);
            Assert.Equal(entity.CreatedOn, alert.DqtCreatedOn);
            Assert.Equal(entity.ModifiedOn, alert.DqtModifiedOn);
            Assert.Equal(expectedAlertType.AlertTypeId, alert.AlertTypeId);
            Assert.Equal(entity.dfeta_SanctionDetails, alert.Details);
            Assert.Equal(entity.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), alert.StartDate);
            Assert.Equal(entity.dfeta_EndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), alert.EndDate);
            Assert.Equal(entity.dfeta_DetailsLink, alert.ExternalLink);
        });
    }

    private async Task AssertAlertEventMatchesEntity(
        dfeta_sanction entity,
        EventModels.Alert eventModel,
        bool expectMigrationMappingsApplied)
    {
        Assert.Equal(entity.Id, eventModel.AlertId);
        Assert.Equal(entity.dfeta_SanctionDetails, eventModel.Details);
        Assert.Equal(entity.dfeta_DetailsLink, eventModel.ExternalLink);
        Assert.Equal(entity.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), eventModel.StartDate);
        Assert.Equal(entity.dfeta_EndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), eventModel.EndDate);

        if (expectMigrationMappingsApplied)
        {
            Assert.Null(eventModel.DqtSpent);
            Assert.Null(eventModel.DqtSanctionCode);

            var sanctionCodes = await TestData.ReferenceDataCache.GetSanctionCodes(activeOnly: false);
            var alertTypes = await TestData.ReferenceDataCache.GetAlertTypes();

            var expectedAlertTypeId = entity.dfeta_SanctionCodeId?.Id is Guid sanctionCodeId ?
                alertTypes.SingleOrDefault(t => t.DqtSanctionCode == sanctionCodes.Single(sc => sc.Id == sanctionCodeId).dfeta_Value)?.AlertTypeId :
                null;

            Assert.Equal(expectedAlertTypeId, eventModel.AlertTypeId);
        }
        else
        {
            Assert.Equal(entity.dfeta_Spent, eventModel.DqtSpent);
            Assert.Null(eventModel.AlertTypeId);
        }
    }

    private Task<EventBase[]> GetEventsForAlert(Guid alertId) =>
        DbFixture.WithDbContext(async dbContext =>
        {
            var results = await dbContext.Database.SqlQuery<AlertEventQueryResult>(
                $"""
                SELECT e.event_name, e.payload
                FROM events as e
                WHERE (e.payload -> 'Alert' ->> 'AlertId')::uuid = {alertId}
                ORDER BY e.created
                """).ToArrayAsync();

            return results.Select(r => EventBase.Deserialize(r.Payload, r.EventName)).ToArray();
        });

    private class AlertEventQueryResult
    {
        public required string EventName { get; set; }
        public required string Payload { get; set; }
    }
}

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
    [InlineData(true, dfeta_InductionStatus.Exempt, InductionStatus.Exempt)]
    [InlineData(true, dfeta_InductionStatus.PassedinWales, InductionStatus.Exempt)]
    [InlineData(true, dfeta_InductionStatus.Fail, InductionStatus.Failed)]
    [InlineData(true, dfeta_InductionStatus.FailedinWales, InductionStatus.FailedInWales)]
    [InlineData(true, dfeta_InductionStatus.InProgress, InductionStatus.InProgress)]
    [InlineData(true, dfeta_InductionStatus.InductionExtended, InductionStatus.InProgress)]
    [InlineData(true, dfeta_InductionStatus.NotYetCompleted, InductionStatus.InProgress)]
    [InlineData(true, dfeta_InductionStatus.Pass, InductionStatus.Passed)]
    [InlineData(true, dfeta_InductionStatus.RequiredtoComplete, InductionStatus.RequiredToComplete)]
    [InlineData(false, dfeta_InductionStatus.Exempt, InductionStatus.Exempt)]
    [InlineData(false, dfeta_InductionStatus.PassedinWales, InductionStatus.Exempt)]
    [InlineData(false, dfeta_InductionStatus.Fail, InductionStatus.Failed)]
    [InlineData(false, dfeta_InductionStatus.FailedinWales, InductionStatus.FailedInWales)]
    [InlineData(false, dfeta_InductionStatus.InProgress, InductionStatus.InProgress)]
    [InlineData(false, dfeta_InductionStatus.InductionExtended, InductionStatus.InProgress)]
    [InlineData(false, dfeta_InductionStatus.NotYetCompleted, InductionStatus.InProgress)]
    [InlineData(false, dfeta_InductionStatus.Pass, InductionStatus.Passed)]
    [InlineData(false, dfeta_InductionStatus.RequiredtoComplete, InductionStatus.RequiredToComplete)]
    public async Task SyncInductionsAsync_WithInduction_UpdatesPersonRecord(bool personAlreadySynced, dfeta_InductionStatus dqtInductionStatus, InductionStatus expectedTrsInductionStatus)
    {
        // Arrange
        var inductionId = Guid.NewGuid();
        var inductionExemptionReason = dqtInductionStatus == dfeta_InductionStatus.Exempt ? dfeta_InductionExemptionReason.Exempt : (dfeta_InductionExemptionReason?)null;
        var inductionStartDate = Clock.Today.AddYears(-1);
        var inductionEndDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithSyncOverride(personAlreadySynced)
                .WithDqtInduction(dqtInductionStatus, inductionExemptionReason, inductionStartDate, inductionEndDate));
        var inductionAuditDetails = new AuditDetailCollection();
        var entity = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, inductionStartDate, inductionEndDate, dqtInductionStatus, inductionExemptionReason);
        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);
        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { entity.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [entity], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(expectedTrsInductionStatus, updatedPerson!.InductionStatus);
            Assert.Equal(inductionStartDate, updatedPerson.InductionStartDate);
            Assert.Equal(inductionEndDate, updatedPerson.InductionCompletedDate);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SyncInductionsAsync_WithContactOnlyInductionStatus_UpdatesPersonRecord(bool personAlreadySynced)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithSyncOverride(personAlreadySynced));

        var auditDetails = new AuditDetailCollection();
        auditDetails.Add(person.DqtContactAuditDetail);

        var updatedContact = await CreateUpdatedContactEntityVersion(
            person.Contact,
            auditDetails,
            dfeta_InductionStatus.RequiredtoComplete);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { person.ContactId, auditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([updatedContact], [], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(InductionStatus.RequiredToComplete, updatedPerson!.InductionStatus);
        });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithExistingDqtInduction_UpdatesPersonRecord()
    {
        // Arrange
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var inductionStartDate = Clock.Today.AddYears(-1);

        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQts()
                .WithDqtInduction(inductionStatus, null, inductionStartDate, null)
                .WithSyncOverride(false));

        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);

        var inductionId = person.DqtInductions.Single().InductionId;
        using var ctx = new DqtCrmServiceContext(TestData.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == inductionId);
        var inductionAuditDetails = new AuditDetailCollection();

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { inductionId, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };


        // Act
        await Helper.SyncInductionsAsync([person.Contact], [induction!], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(inductionStatus.ToInductionStatus(), updatedPerson!.InductionStatus);
            Assert.Equal(inductionStartDate, updatedPerson.InductionStartDate);
        });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithQtlsButNotExemptAndIgnoreInvalidSetToFalse_ThrowsException()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQts()
                .WithQtlsDate(Clock.Today)
                .WithSyncOverride(true));

        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);

        var inductionAuditDetails = new AuditDetailCollection();
        var induction = await CreateNewInductionEntityVersion(Guid.NewGuid(), person.Contact, inductionAuditDetails, null, null, dfeta_InductionStatus.RequiredtoComplete, null);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { induction.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        var exception = await Record.ExceptionAsync(() => Helper.SyncInductionsAsync([person.Contact], [induction], auditDetailsDict, ignoreInvalid: false, createMigratedEvent: false, dryRun: false, CancellationToken.None));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task SyncInductionsAsync_WithQtls_UpdatesPersonRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQtlsDate(Clock.Today)
                .WithSyncOverride(true));

        var auditDetails = new AuditDetailCollection();
        auditDetails.Add(person.DqtContactAuditDetail);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { person.ContactId, auditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleOrDefaultAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(InductionStatus.Exempt, updatedPerson!.InductionStatus);
        });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithDqtCreateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { initialVersion.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [initialVersion], auditDetailsDict, ignoreInvalid: false, createMigratedEvent: true, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var createdEvent = Assert.IsType<DqtInductionCreatedEvent>(e);
                Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), createdEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, createdEvent.PersonId);
                AssertInductionEventMatchesEntity(initialVersion, createdEvent.Induction);
            },
            e =>
            {
                var migratedEvent = Assert.IsType<InductionMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                AssertInductionEventMatchesEntity(initialVersion, migratedEvent.DqtInduction);
                Assert.Equal(initialVersion.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), migratedEvent.InductionStartDate);
                Assert.Equal(initialVersion.dfeta_CompletionDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), migratedEvent.InductionCompletedDate);
                Assert.Equal(initialVersion.dfeta_InductionStatus.ToInductionStatus().ToString(), migratedEvent.InductionStatus);
                Assert.Equal(InductionExemptionReasons.None.ToString(), migratedEvent.InductionExemptionReason);
                return Task.CompletedTask;
            });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithNoDqtAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, addCreateAudit: false);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { initialVersion.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [initialVersion], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: true, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var importedEvent = Assert.IsType<DqtInductionImportedEvent>(e);
                Assert.Equal(Clock.UtcNow, importedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), importedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, importedEvent.PersonId);
                AssertInductionEventMatchesEntity(initialVersion, importedEvent.Induction);
            },
            e =>
            {
                var migratedEvent = Assert.IsType<InductionMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                AssertInductionEventMatchesEntity(initialVersion, migratedEvent.DqtInduction);
                Assert.Equal(initialVersion.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), migratedEvent.InductionStartDate);
                Assert.Equal(initialVersion.dfeta_CompletionDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), migratedEvent.InductionCompletedDate);
                Assert.Equal(initialVersion.dfeta_InductionStatus.ToInductionStatus().ToString(), migratedEvent.InductionStatus);
                Assert.Equal(InductionExemptionReasons.None.ToString(), migratedEvent.InductionExemptionReason);
                return Task.CompletedTask;
            });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithNoDqtCreateButWithUpdateAudits_CreatesExpectedEvents()
    {
        // Many migrated records in DQT don't have a 'Create' audit record, since auditing was turned on after migration.
        // In that case, TrsDataSyncHelper will take the current version and 'un-apply' every Update audit in reverse,
        // leaving the initial version at the end. From that an AlertDqtImported event is created.
        // This test is exercising that scenario.
        // The Updates created here are deliberately partial updates to verify that the original version of the entity can
        // be reconstructed from multiple Update audits.

        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, addCreateAudit: false);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { initialVersion.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        var created = Clock.UtcNow;

        Clock.Advance();
        var intermediateVersion = await CreateUpdatedInductionEntityVersion(initialVersion, inductionAuditDetails, DqtInductionUpdatedEventChanges.StartDate);

        Clock.Advance();
        var updatedVersion = await CreateUpdatedInductionEntityVersion(intermediateVersion, inductionAuditDetails, DqtInductionUpdatedEventChanges.CompletionDate);

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [updatedVersion], auditDetailsDict, ignoreInvalid: false, createMigratedEvent: true, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);

        await Assert.CollectionAsync(
            events,
            async e =>
            {
                var importedEvent = Assert.IsType<DqtInductionImportedEvent>(e);
                Assert.Equal(created, importedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), importedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, importedEvent.PersonId);
                AssertInductionEventMatchesEntity(initialVersion, importedEvent.Induction);
            },
            e =>
            {
                // Checking UpdatedEvent details is covered elsewhere
                Assert.IsType<DqtInductionUpdatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                // Checking UpdatedEvent details is covered elsewhere
                Assert.IsType<DqtInductionUpdatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                var migratedEvent = Assert.IsType<InductionMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                AssertInductionEventMatchesEntity(updatedVersion, migratedEvent.DqtInduction);
                Assert.Equal(updatedVersion.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), migratedEvent.InductionStartDate);
                Assert.Equal(updatedVersion.dfeta_CompletionDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), migratedEvent.InductionCompletedDate);
                Assert.Equal(updatedVersion.dfeta_InductionStatus.ToInductionStatus().ToString(), migratedEvent.InductionStatus);
                Assert.Equal(InductionExemptionReasons.None.ToString(), migratedEvent.InductionExemptionReason);
                return Task.CompletedTask;
            });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithDqtUpdateAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails);

        Clock.Advance();
        var updatedVersion = await CreateUpdatedInductionEntityVersion(initialVersion, inductionAuditDetails);

        var updatedContact = await CreateUpdatedContactEntityVersion(person.Contact, contactAuditDetails, updatedVersion.dfeta_InductionStatus!.Value);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { initialVersion.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([updatedContact], [updatedVersion], auditDetailsDict, ignoreInvalid: false, createMigratedEvent: true, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                Assert.IsType<DqtInductionCreatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var updatedEvent = Assert.IsType<DqtInductionUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, updatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), updatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, updatedEvent.PersonId);
                AssertInductionEventMatchesEntity(updatedVersion, updatedEvent.Induction);
                Assert.Equal(GetChanges(initialVersion, updatedVersion), updatedEvent.Changes);
            },
            e =>
            {
                var migratedEvent = Assert.IsType<InductionMigratedEvent>(e);
                Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
                Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
                Assert.Equal(person.PersonId, migratedEvent.PersonId);
                AssertInductionEventMatchesEntity(updatedVersion, migratedEvent.DqtInduction);
                return Task.CompletedTask;
            });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithDqtDeactivatedAudit_CreatesExpectedEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var induction = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersionAsync(induction, dfeta_induction.EntityLogicalName, inductionAuditDetails);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { induction.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [deactivatedVersion], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                Assert.IsType<DqtInductionCreatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var deactivatedEvent = Assert.IsType<DqtInductionDeactivatedEvent>(e);
                Assert.Equal(Clock.UtcNow, deactivatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), deactivatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, deactivatedEvent.PersonId);
                AssertInductionEventMatchesEntity(deactivatedVersion, deactivatedEvent.Induction);
            });
    }

    [Fact]
    public async Task SyncInductionsAsync_WithDqtReactivatedAudit_CreatesExpectedEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var induction = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails);

        Clock.Advance();
        var deactivatedVersion = await CreateDeactivatedEntityVersionAsync(induction, dfeta_induction.EntityLogicalName, inductionAuditDetails);

        Clock.Advance();
        var reactivatedVersion = await CreateReactivatedEntityVersionAsync(deactivatedVersion, dfeta_induction.EntityLogicalName, inductionAuditDetails);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { induction.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [deactivatedVersion], auditDetailsDict, ignoreInvalid: true, createMigratedEvent: false, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);

        await Assert.CollectionAsync(
            events,
            e =>
            {
                Assert.IsType<DqtInductionCreatedEvent>(e);
                return Task.CompletedTask;
            },
            e =>
            {
                Assert.IsType<DqtInductionDeactivatedEvent>(e);
                return Task.CompletedTask;
            },
            async e =>
            {
                var reactivatedEvent = Assert.IsType<DqtInductionReactivatedEvent>(e);
                Assert.Equal(Clock.UtcNow, reactivatedEvent.CreatedUtc);
                Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), reactivatedEvent.RaisedBy.DqtUserId);
                Assert.Equal(person.PersonId, reactivatedEvent.PersonId);
                AssertInductionEventMatchesEntity(reactivatedVersion, reactivatedEvent.Induction);
            });
    }

    private async Task<dfeta_induction> CreateNewInductionEntityVersion(
        Guid inductionId,
        Contact contact,
        AuditDetailCollection auditDetailCollection,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        dfeta_InductionStatus? status = null,
        dfeta_InductionExemptionReason? exemptionReason = null,
        bool addCreateAudit = true)
    {
        Debug.Assert(auditDetailCollection.Count == 0);

        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();
        var createdOn = Clock.UtcNow;
        var modifiedOn = Clock.UtcNow;
        var state = dfeta_inductionState.Active;

        var newInduction = new dfeta_induction()
        {
            Id = inductionId,
            dfeta_PersonId = contact.Id.ToEntityReference(Contact.EntityLogicalName),
            CreatedOn = createdOn,
            CreatedBy = currentDqtUser,
            ModifiedOn = modifiedOn,
            StateCode = state,
            dfeta_StartDate = startDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_CompletionDate = endDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_InductionStatus = status,
            dfeta_InductionExemptionReason = exemptionReason
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
                NewValue = newInduction.Clone()
            });
        }

        return newInduction;
    }

    private async Task<dfeta_induction> CreateUpdatedInductionEntityVersion(
        dfeta_induction existingInduction,
        AuditDetailCollection auditDetailCollection,
        DqtInductionUpdatedEventChanges? changes = null)
    {
        if (changes == DqtInductionUpdatedEventChanges.None)
        {
            throw new ArgumentException("Changes cannot be None.", nameof(changes));
        }

        bool ChangeRequested(DqtInductionUpdatedEventChanges field) =>
            changes is null || changes.Value.HasFlag(field);

        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var existingStartDate = existingInduction.dfeta_StartDate;
        var startDate = ChangeRequested(DqtInductionUpdatedEventChanges.StartDate) ?
            (existingInduction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true) is DateOnly existingStartDateOnly ?
                TestData.GenerateChangedDate(existingStartDateOnly, min: new DateOnly(2020, 4, 1)) :
                TestData.GenerateDate(min: new DateOnly(2020, 4, 1))).ToDateTimeWithDqtBstFix(isLocalTime: true) :
            existingStartDate;

        var existingCompletionDate = existingInduction.dfeta_CompletionDate;
        DateTime? completionDate;

        if (ChangeRequested(DqtInductionUpdatedEventChanges.CompletionDate))
        {
            if (startDate is null)
            {
                throw new InvalidOperationException("Cannot generate a completion date when there is no start date.");
            }

            var startDateOnly = startDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true);

            completionDate = (existingCompletionDate is null ?
                    TestData.GenerateDate(min: startDateOnly.AddDays(1)) :
                    TestData.GenerateChangedDate(existingCompletionDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true), min: startDateOnly.AddDays(1)))
                .ToDateTimeWithDqtBstFix(isLocalTime: true);
        }
        else
        {
            completionDate = null;
        }

        var existingStatus = existingInduction.dfeta_InductionStatus;
        var status = ChangeRequested(DqtInductionUpdatedEventChanges.Status) ?
            TestData.GenerateChangedEnumValue(existingStatus) :
            existingStatus;

        var existingExemptionReason = existingInduction.dfeta_InductionExemptionReason;
        var exemptionReason = ChangeRequested(DqtInductionUpdatedEventChanges.ExemptionReason) ?
            TestData.GenerateChangedEnumValue(existingExemptionReason) :
            existingExemptionReason;

        var updatedInduction = existingInduction.Clone<dfeta_induction>();
        updatedInduction.ModifiedOn = Clock.UtcNow;
        updatedInduction.dfeta_StartDate = startDate;
        updatedInduction.dfeta_CompletionDate = completionDate;
        updatedInduction.dfeta_InductionStatus = status;
        updatedInduction.dfeta_InductionExemptionReason = exemptionReason;

        var changedAttrs = (
            from newAttr in updatedInduction.Attributes
            join oldAttr in existingInduction.Attributes on newAttr.Key equals oldAttr.Key
            where !AttributeValuesEqual(newAttr.Value, oldAttr.Value)
            select newAttr.Key).ToArray();

        var oldValue = new Entity(dfeta_induction.EntityLogicalName, existingInduction.Id);
        Array.ForEach(changedAttrs, a => oldValue.Attributes[a] = existingInduction.Attributes[a]);

        var newValue = new Entity(dfeta_induction.EntityLogicalName, existingInduction.Id);
        Array.ForEach(changedAttrs, a => newValue.Attributes[a] = updatedInduction.Attributes[a]);


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

        return updatedInduction;

        static bool AttributeValuesEqual(object? a, object? b) =>
            a is null && b is null ||
            (a is not null && b is not null && a.Equals(b));
    }

    private async Task<Contact> CreateUpdatedContactEntityVersion(
        Contact existingContact,
        AuditDetailCollection auditDetailCollection,
        dfeta_InductionStatus updatedInductionStatus)
    {
        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var updatedContact = existingContact.Clone<Contact>();
        updatedContact.ModifiedOn = Clock.UtcNow;
        updatedContact.dfeta_InductionStatus = updatedInductionStatus;

        var oldValue = new Entity(Contact.EntityLogicalName, existingContact.Id);
        oldValue.Attributes[Contact.Fields.ModifiedOn] = existingContact.Attributes[Contact.Fields.ModifiedOn];
        oldValue.Attributes[Contact.Fields.dfeta_InductionStatus] = existingContact.GetAttributeValue<dfeta_InductionStatus?>(Contact.Fields.dfeta_InductionStatus);

        var newValue = new Entity(Contact.EntityLogicalName, existingContact.Id);
        newValue.Attributes[Contact.Fields.ModifiedOn] = updatedContact.Attributes[Contact.Fields.ModifiedOn];
        newValue.Attributes[Contact.Fields.dfeta_InductionStatus] = updatedContact.Attributes[Contact.Fields.dfeta_InductionStatus];

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

        return updatedContact;
    }

    private static DqtInductionUpdatedEventChanges GetChanges(dfeta_induction first, dfeta_induction second) =>
        DqtInductionUpdatedEventChanges.None |
        (first.dfeta_StartDate != second.dfeta_StartDate ? DqtInductionUpdatedEventChanges.StartDate : DqtInductionUpdatedEventChanges.None) |
        (first.dfeta_CompletionDate != second.dfeta_CompletionDate ? DqtInductionUpdatedEventChanges.CompletionDate : DqtInductionUpdatedEventChanges.None) |
        (first.dfeta_InductionStatus != second.dfeta_InductionStatus ? DqtInductionUpdatedEventChanges.Status : DqtInductionUpdatedEventChanges.None) |
        (first.dfeta_InductionExemptionReason != second.dfeta_InductionExemptionReason ? DqtInductionUpdatedEventChanges.ExemptionReason : DqtInductionUpdatedEventChanges.None);

    private void AssertInductionEventMatchesEntity(
        dfeta_induction entity,
        EventModels.DqtInduction eventModel)
    {
        Assert.Equal(entity.Id, eventModel.InductionId);
        Assert.Equal(entity.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), eventModel.StartDate);
        Assert.Equal(entity.dfeta_CompletionDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), eventModel.CompletionDate);
        Assert.Equal(entity.dfeta_InductionStatus.ToString(), eventModel.InductionStatus);
        Assert.Equal(entity.dfeta_InductionExemptionReason.ToString(), eventModel.InductionExemptionReason);
    }

    private Task<EventBase[]> GetEventsForInduction(Guid inductionId) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            var results = await dbContext.Database.SqlQuery<AlertEventQueryResult>(
                $"""
                SELECT e.event_name, e.payload
                FROM events as e
                WHERE (e.payload -> 'Induction' ->> 'InductionId')::uuid = {inductionId}
                  OR (e.payload -> 'DqtInduction' ->> 'InductionId')::uuid = {inductionId}
                ORDER BY e.created, (CASE WHEN e.event_name = 'InductionMigratedEvent' THEN 1 ELSE 0 END)
                """).ToArrayAsync();

            return results.Select(r => EventBase.Deserialize(r.Payload, r.EventName)).ToArray();
        });

    private class InductionEventQueryResult
    {
        public required string EventName { get; set; }
        public required string Payload { get; set; }
    }
}

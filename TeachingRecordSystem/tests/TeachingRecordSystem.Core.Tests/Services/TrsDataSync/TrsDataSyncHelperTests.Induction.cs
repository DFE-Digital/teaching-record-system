using System.Diagnostics;
using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Optional.Unsafe;
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
        await Helper.SyncInductionsAsync([person.Contact], [entity], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

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
    [InlineData(dfeta_InductionExemptionReason.Exempt, "a5faff9f-29ce-4a6b-a7b8-0c1f57f15920")]
    [InlineData(dfeta_InductionExemptionReason.ExemptDataLossErrorCriteria, "204f86eb-0383-40eb-b793-6fccb76ecee2")]
    [InlineData(dfeta_InductionExemptionReason.HasoriseligibleforfullregistrationinScotland, "a112e691-1694-46a7-8f33-5ec5b845c181")]
    [InlineData(dfeta_InductionExemptionReason.OverseasTrainedTeacher, "4c97e211-10d2-4c63-8da9-b0fcebe7f2f9")]
    [InlineData(dfeta_InductionExemptionReason.Qualifiedbefore07May1999, "5a80cee8-98a8-426b-8422-b0e81cb49b36")]
    [InlineData(dfeta_InductionExemptionReason.Qualifiedbetween07May1999and01April2003FirstpostwasinWalesandlastedaminimumoftwoterms, "15014084-2d8d-4f51-9198-b0e1881f8896")]
    [InlineData(dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute, "e7118bab-c2b1-4fe8-ad3f-4095d73f5b85")]
    [InlineData(dfeta_InductionExemptionReason.QualifiedthroughFEroutebetween01Sep2001and01Sep2004, "0997ab13-7412-4560-8191-e51ed4d58d2a")]
    [InlineData(dfeta_InductionExemptionReason.RegisteredTeacher_havingatleasttwoyearsfulltimeteachingexperience, "42bb7bbc-a92c-4886-b319-3c1a5eac319a")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedinductioninGuernsey, "fea2db23-93e0-49af-96fd-83c815c17c0b")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedinductioninIsleOfMan, "e5c3847d-8fb6-4b31-8726-812392da8c5c")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedinductioninJersey, "243b21a8-0be4-4af5-8874-85944357e7f8")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedinductioninNorthernIreland, "3471ab35-e6e4-4fa9-a72b-b8bd113df591")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedinductioninServiceChildrensEducationschoolsinGermanyorCyprus, "7d17d904-c1c6-451b-9e09-031314bd35f7")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedinductioninWales, "39550fa9-3147-489d-b808-4feea7f7f979")]
    [InlineData(dfeta_InductionExemptionReason.SuccessfullycompletedprobationaryperiodinGibraltar, "a751494a-7e7a-4836-96cb-00b9ed6e1b5f")]
    [InlineData(dfeta_InductionExemptionReason.TeacherhasbeenawardedQTLSandisexemptprovidedtheymaintaintheirmembershipwiththeSocietyforEducationandTraining, "35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db")]
    public async Task SyncInductionsAsync_WithInductionExemptionReason_MapsToTrsAsExpected(dfeta_InductionExemptionReason dqtInductionExemptionReason, string expectedTrsInductionExemptionReason)
    {
        // Arrange
        var expectedTrsInductionExemptionReasonId = new Guid(expectedTrsInductionExemptionReason);
        var inductionId = Guid.NewGuid();
        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithSyncOverride(false)
                .WithDqtInduction(dfeta_InductionStatus.Exempt, dqtInductionExemptionReason, null, null));
        var inductionAuditDetails = new AuditDetailCollection();
        var entity = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, status: dfeta_InductionStatus.Exempt, exemptionReason: dqtInductionExemptionReason);
        var contactAuditDetails = new AuditDetailCollection();
        contactAuditDetails.Add(person.DqtContactAuditDetail);
        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { entity.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [entity], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

        // Assert
        await DbFixture.WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.DqtContactId == person.ContactId);
            Assert.Equal(expectedTrsInductionExemptionReasonId, updatedPerson!.InductionExemptionReasonIds[0]);
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
        await Helper.SyncInductionsAsync([updatedContact], [], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

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
        inductionAuditDetails.Add(await GenerateCreateAuditFromEntity(induction!));

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { inductionId, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([person.Contact], [induction!], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

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
        var exception = await Record.ExceptionAsync(() => Helper.SyncInductionsAsync([person.Contact], [induction], auditDetailsDict, ignoreInvalid: false, dryRun: false, CancellationToken.None));

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
        await Helper.SyncInductionsAsync([person.Contact], [], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

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
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var startDate = new DateOnly(2023, 02, 01);
        var completionDate = new DateOnly(2023, 12, 10);
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, startDate, completionDate, inductionStatus);

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        var contact = await CreateUpdatedContactEntityVersion(person.Contact, contactAuditDetails, inductionStatus);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { initialVersion.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([contact], [initialVersion], auditDetailsDict, ignoreInvalid: false, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);
        Assert.Single(events);
        var createdEvent = Assert.IsType<DqtInductionCreatedEvent>(events[0]);
        Assert.Equal(Clock.UtcNow, createdEvent.CreatedUtc);
        Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), createdEvent.RaisedBy.DqtUserId);
        Assert.Equal(person.PersonId, createdEvent.PersonId);
        Assert.Equal(inductionId, createdEvent.Induction.InductionId);
        Assert.Equal(inductionStatus.GetMetadata().Name, createdEvent.Induction.InductionStatus.ValueOrFailure());
        Assert.Equal(startDate, createdEvent.Induction.StartDate.ValueOrFailure());
        Assert.Equal(completionDate, createdEvent.Induction.CompletionDate.ValueOrFailure());
    }

    [Fact]
    public async Task SyncInductionsAsync_WithNoDqtAudit_CreatesExpectedEvents()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn());
        var contactAuditDetails = new AuditDetailCollection();

        var inductionId = Guid.NewGuid();
        var inductionAuditDetails = new AuditDetailCollection();
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var startDate = new DateOnly(2023, 02, 01);
        var completionDate = new DateOnly(2023, 12, 10);
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, startDate, completionDate, inductionStatus, addCreateAudit: false);

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        var contact = await CreateUpdatedContactEntityVersion(person.Contact, contactAuditDetails, inductionStatus);

        var auditDetailsDict = new Dictionary<Guid, AuditDetailCollection>()
        {
            { initialVersion.Id, inductionAuditDetails },
            { person.ContactId, contactAuditDetails }
        };

        // Act
        await Helper.SyncInductionsAsync([contact], [initialVersion], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);
        Assert.Single(events);
        var importedEvent = Assert.IsType<DqtInductionImportedEvent>(events[0]);
        Assert.Equal(Clock.UtcNow, importedEvent.CreatedUtc);
        Assert.Equal(await TestData.GetCurrentCrmUserIdAsync(), importedEvent.RaisedBy.DqtUserId);
        Assert.Equal(person.PersonId, importedEvent.PersonId);
        Assert.Equal(inductionId, importedEvent.Induction.InductionId);
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
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var startDate = new DateOnly(2023, 02, 01);
        var completionDate = new DateOnly(2023, 12, 10);
        var initialVersion = await CreateNewInductionEntityVersion(inductionId, person.Contact, inductionAuditDetails, startDate, completionDate, inductionStatus, addCreateAudit: false);

        // Keep the contact induction status in sync with dfeta_induction otherwise the sync will fail
        var contact = await CreateUpdatedContactEntityVersion(person.Contact, contactAuditDetails, inductionStatus);

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
        await Helper.SyncInductionsAsync([contact], [updatedVersion], auditDetailsDict, ignoreInvalid: false, dryRun: false, CancellationToken.None);

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
                Assert.Equal(inductionId, importedEvent.Induction.InductionId);
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
        await Helper.SyncInductionsAsync([updatedContact], [updatedVersion], auditDetailsDict, ignoreInvalid: false, dryRun: false, CancellationToken.None);

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
                Assert.Equal(inductionId, updatedEvent.Induction.InductionId);
                Assert.Equal(updatedVersion.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), updatedEvent.Induction.StartDate.ValueOrFailure());
                Assert.Equal(updatedVersion.dfeta_CompletionDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), updatedEvent.Induction.CompletionDate.ValueOrFailure());
                Assert.Equal(updatedVersion.dfeta_InductionStatus!.Value.GetMetadata().Name, updatedEvent.Induction.InductionStatus.ValueOrFailure());
                Assert.Equal(GetChanges(initialVersion, updatedVersion), updatedEvent.Changes);
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
        await Helper.SyncInductionsAsync([person.Contact], [deactivatedVersion], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

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
                Assert.Equal(inductionId, deactivatedEvent.Induction.InductionId);
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
        await Helper.SyncInductionsAsync([person.Contact], [deactivatedVersion], auditDetailsDict, ignoreInvalid: true, dryRun: false, CancellationToken.None);

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
                Assert.Equal(inductionId, reactivatedEvent.Induction.InductionId);
            });
    }

    [Fact]
    public async Task MigrateInductionsAsync_WithDqtInductions_CreatesExpectedEvents()
    {
        // Arrange
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var inductionStartDate = Clock.Today.AddYears(-1);
        var inductionCompletionDate = Clock.Today.AddDays(-10);

        var person = await TestData.CreatePersonAsync(
            p => p.WithTrn()
                .WithQts()
                .WithDqtInduction(inductionStatus, null, inductionStartDate, inductionCompletionDate)
                .WithSyncOverride(false));
        var inductionId = person.DqtInductions.Single().InductionId;

        // Act
        await Helper.MigrateInductionsAsync([person.Contact], ignoreInvalid: true, dryRun: false, CancellationToken.None);

        // Assert
        var events = await GetEventsForInduction(inductionId);
        Assert.Single(events);
        var migratedEvent = Assert.IsType<InductionMigratedEvent>(events[0]);
        Assert.Equal(Clock.UtcNow, migratedEvent.CreatedUtc);
        Assert.Equal(Core.DataStore.Postgres.Models.SystemUser.SystemUserId, migratedEvent.RaisedBy.UserId);
        Assert.Equal(person.PersonId, migratedEvent.PersonId);
        Assert.Equal(inductionId, migratedEvent.DqtInduction.InductionId);
        Assert.Equal(inductionStatus.GetMetadata().Name, migratedEvent.DqtInduction.InductionStatus.ValueOrFailure());
        Assert.Equal(inductionStartDate, migratedEvent.DqtInduction.StartDate.ValueOrFailure());
        Assert.Equal(inductionCompletionDate, migratedEvent.DqtInduction.CompletionDate.ValueOrFailure());
        Assert.Equal(inductionStartDate, migratedEvent.InductionStartDate);
        Assert.Equal(inductionCompletionDate, migratedEvent.InductionCompletedDate);
        Assert.Equal(inductionStatus.ToInductionStatus(), migratedEvent.InductionStatus);
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
                OldValue = new Entity(dfeta_induction.EntityLogicalName),
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
            TestData.GenerateChangedEnumValue(existingExemptionReason, excluding: [dfeta_InductionExemptionReason.Extendedonappeal, dfeta_InductionExemptionReason.QualifiedthroughIndependentroutebetween01Oct2000and01Sep2004]) :
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
        dfeta_InductionStatus? updatedInductionStatus = null,
        DateOnly? updatedQtlsDate = null)
    {
        var currentDqtUser = await TestData.GetCurrentCrmUserAsync();

        var updatedContact = existingContact.Clone<Contact>();
        updatedContact.ModifiedOn = Clock.UtcNow;

        if (updatedQtlsDate is not null)
        {
            updatedContact.dfeta_qtlsdate = updatedQtlsDate.Value.ToDateTimeWithDqtBstFix(isLocalTime: true);

            var oldValueQtlsDate = new Entity(Contact.EntityLogicalName, existingContact.Id);
            oldValueQtlsDate.Attributes[Contact.Fields.ModifiedOn] = existingContact.Attributes[Contact.Fields.ModifiedOn];
            oldValueQtlsDate.Attributes[Contact.Fields.dfeta_qtlsdate] = existingContact.GetAttributeValue<DateTime?>(Contact.Fields.dfeta_qtlsdate);

            var newValueQtlsDate = new Entity(Contact.EntityLogicalName, existingContact.Id);
            newValueQtlsDate.Attributes[Contact.Fields.ModifiedOn] = updatedContact.Attributes[Contact.Fields.ModifiedOn];
            newValueQtlsDate.Attributes[Contact.Fields.dfeta_qtlsdate] = updatedContact.Attributes[Contact.Fields.dfeta_qtlsdate];

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
                OldValue = oldValueQtlsDate,
                NewValue = newValueQtlsDate
            });
        }

        if (updatedInductionStatus is not null)
        {
            updatedContact.dfeta_InductionStatus = updatedInductionStatus;

            var oldValueStatus = new Entity(Contact.EntityLogicalName, existingContact.Id);
            oldValueStatus.Attributes[Contact.Fields.ModifiedOn] = existingContact.Attributes[Contact.Fields.ModifiedOn];
            oldValueStatus.Attributes[Contact.Fields.dfeta_InductionStatus] = existingContact.GetAttributeValue<dfeta_InductionStatus?>(Contact.Fields.dfeta_InductionStatus);

            var newValueStatus = new Entity(Contact.EntityLogicalName, existingContact.Id);
            newValueStatus.Attributes[Contact.Fields.ModifiedOn] = updatedContact.Attributes[Contact.Fields.ModifiedOn];
            newValueStatus.Attributes[Contact.Fields.dfeta_InductionStatus] = updatedContact.Attributes[Contact.Fields.dfeta_InductionStatus];

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
                OldValue = oldValueStatus,
                NewValue = newValueStatus
            });
        }

        return updatedContact;
    }

    private async Task<AttributeAuditDetail> GenerateCreateAuditFromEntity(dfeta_induction induction)
    {
        return new AttributeAuditDetail()
        {
            AuditRecord = new Audit()
            {
                Action = Audit_Action.Create,
                AuditId = Guid.NewGuid(),
                CreatedOn = Clock.UtcNow,
                Id = Guid.NewGuid(),
                Operation = Audit_Operation.Create,
                UserId = await TestData.GetCurrentCrmUserAsync()
            },
            OldValue = new Entity(dfeta_induction.EntityLogicalName),
            NewValue = induction.Clone()
        };
    }

    private static DqtInductionUpdatedEventChanges GetChanges(dfeta_induction first, dfeta_induction second) =>
        DqtInductionUpdatedEventChanges.None |
        (first.dfeta_StartDate != second.dfeta_StartDate ? DqtInductionUpdatedEventChanges.StartDate : DqtInductionUpdatedEventChanges.None) |
        (first.dfeta_CompletionDate != second.dfeta_CompletionDate ? DqtInductionUpdatedEventChanges.CompletionDate : DqtInductionUpdatedEventChanges.None) |
        (first.dfeta_InductionStatus != second.dfeta_InductionStatus ? DqtInductionUpdatedEventChanges.Status : DqtInductionUpdatedEventChanges.None) |
        (first.dfeta_InductionExemptionReason != second.dfeta_InductionExemptionReason ? DqtInductionUpdatedEventChanges.ExemptionReason : DqtInductionUpdatedEventChanges.None);

    private Task<EventBase[]> GetEventsForInduction(Guid inductionId) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            var results = await dbContext.Database.SqlQuery<EventQueryResult>(
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

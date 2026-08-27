using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillRouteToProfessionalStatusProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyCreatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = user.UserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(),
            ChangeReason = "Some reason",
            ChangeReasonDetail = "Some detail",
            EvidenceFile = null,
            AdditionalInformation = "Some additional information",
            Changes = LegacyEvents.RouteToProfessionalStatusCreatedEventChanges.PersonQtsDate,
            PersonAttributes = CreatePersonAttributes(qtsDate: new DateOnly(2024, 1, 1)),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
            Induction = CreateInduction(),
            OldInduction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(RouteToProfessionalStatusCreatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));

            var createdEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, createdEvent.PersonId);
            Assert.Equal(RouteToProfessionalStatusCreatedEventChanges.PersonQtsDate, createdEvent.Changes);
            Assert.Equal(new DateOnly(2024, 1, 1), createdEvent.PersonAttributes.QtsDate);
            Assert.Null(createdEvent.OldPersonAttributes.QtsDate);
            Assert.NotNull(createdEvent.Induction);
            Assert.NotNull(createdEvent.OldInduction);
            Assert.Equal(RouteToProfessionalStatusStatus.InTraining, createdEvent.RouteToProfessionalStatus.Status);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, process.ProcessType);
            Assert.Equal(user.UserId, process.UserId);
            Assert.Null(process.DqtUserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));

            // The reason, its detail and the additional information move off the event and onto the process.
            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Equal("Some reason", changeReason.Reason);
            Assert.Equal("Some detail", changeReason.Details);
            Assert.Equal("Some additional information", changeReason.AdditionalInformation);
            Assert.Null(changeReason.EvidenceFile);
        });
    }

    [Fact]
    public async Task Execute_LegacyCreatedEventWithEvidenceFile_PutsTheEvidenceFileOnTheProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var fileId = Guid.NewGuid();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(),
            ChangeReason = "Some reason",
            ChangeReasonDetail = null,
            EvidenceFile = new EventModels.File { FileId = fileId, Name = "evidence.pdf" },
            AdditionalInformation = null,
            Changes = LegacyEvents.RouteToProfessionalStatusCreatedEventChanges.None,
            PersonAttributes = CreatePersonAttributes(qtsDate: null),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
            Induction = CreateInduction(),
            OldInduction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Equal(fileId, changeReason.EvidenceFile?.FileId);
            Assert.Equal("evidence.pdf", changeReason.EvidenceFile?.Name);
        });
    }

    [Fact]
    public async Task Execute_LegacyUpdatedEvent_CreatesProcessAndProcessEventWithOldRoute()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(status: RouteToProfessionalStatusStatus.Holds),
            OldRouteToProfessionalStatus = CreateRoute(status: RouteToProfessionalStatusStatus.InTraining),
            ChangeReason = null,
            ChangeReasonDetail = null,
            EvidenceFile = null,
            AdditionalInformation = null,
            Changes = LegacyEvents.RouteToProfessionalStatusUpdatedEventChanges.Status,
            PersonAttributes = CreatePersonAttributes(qtsDate: null),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
            Induction = CreateInduction(),
            OldInduction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(RouteToProfessionalStatusUpdatedEvent), processEvent.EventName);

            var updatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(processEvent.Payload);
            Assert.Equal(RouteToProfessionalStatusStatus.Holds, updatedEvent.RouteToProfessionalStatus.Status);
            Assert.Equal(RouteToProfessionalStatusStatus.InTraining, updatedEvent.OldRouteToProfessionalStatus.Status);
            Assert.Equal(RouteToProfessionalStatusUpdatedEventChanges.Status, updatedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, process.ProcessType);

            // Nothing was recorded against the change, so the process has no reason at all.
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyDeletedEvent_MapsTheDeletionReasonOntoTheProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusDeletedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(),
            DeletionReason = "Added in error",
            DeletionReasonDetail = "Some detail",
            EvidenceFile = null,
            AdditionalInformation = null,
            Changes = LegacyEvents.RouteToProfessionalStatusDeletedEventChanges.PersonQtsDate,
            PersonAttributes = CreatePersonAttributes(qtsDate: null),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: new DateOnly(2024, 1, 1)),
            Induction = CreateInduction(),
            OldInduction = CreateInduction()
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(RouteToProfessionalStatusDeletedEvent), processEvent.EventName);

            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(processEvent.Payload);
            Assert.Equal(RouteToProfessionalStatusDeletedEventChanges.PersonQtsDate, deletedEvent.Changes);
            Assert.Equal(new DateOnly(2024, 1, 1), deletedEvent.OldPersonAttributes.QtsDate);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.RouteToProfessionalStatusDeleting, process.ProcessType);

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(process.ChangeReason);
            Assert.Equal("Added in error", changeReason.Reason);
            Assert.Equal("Some detail", changeReason.Details);
        });
    }

    [Fact]
    public async Task Execute_LegacyMigratedEvent_CreatesProcessAndProcessEventWithDqtRecords()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var ittId = Guid.NewGuid();
        var qtsRegistrationId = Guid.NewGuid();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(),
            PersonAttributes = CreatePersonAttributes(qtsDate: new DateOnly(2024, 1, 1)),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
            DqtInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
            {
                InitialTeacherTrainingId = ittId,
                Result = "Pass",
                ProviderName = "Some provider"
            },
            DqtQtsRegistration = new EventModels.DqtQtsRegistration
            {
                QtsRegistrationId = qtsRegistrationId,
                TeacherStatusName = "Qualified teacher"
            },
            DqtQtlsDate = new DateOnly(2023, 6, 1),
            DqtQtlsDateHasBeenSet = true
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(RouteToProfessionalStatusMigratedEvent), processEvent.EventName);

            var migratedEvent = Assert.IsType<RouteToProfessionalStatusMigratedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, migratedEvent.PersonId);
            Assert.Equal(ittId, migratedEvent.DqtInitialTeacherTraining?.InitialTeacherTrainingId);
            Assert.Equal("Pass", migratedEvent.DqtInitialTeacherTraining?.Result);
            Assert.Equal(qtsRegistrationId, migratedEvent.DqtQtsRegistration?.QtsRegistrationId);
            Assert.Equal("Qualified teacher", migratedEvent.DqtQtsRegistration?.TeacherStatusName);
            Assert.Equal(new DateOnly(2023, 6, 1), migratedEvent.DqtQtlsDate);
            Assert.True(migratedEvent.DqtQtlsDateHasBeenSet);
            Assert.Equal(new DateOnly(2024, 1, 1), migratedEvent.PersonAttributes.QtsDate);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.RouteToProfessionalStatusMigratingFromDqt, process.ProcessType);
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventRaisedByDqtUser_PutsTheDqtUserOnTheProcess()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = dqtUser,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(),
            PersonAttributes = CreatePersonAttributes(qtsDate: null),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
            DqtInitialTeacherTraining = null,
            DqtQtsRegistration = null,
            DqtQtlsDate = null,
            DqtQtlsDateHasBeenSet = null
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);

            Assert.Null(process.UserId);
            Assert.Equal(dqtUser.DqtUserId, process.DqtUserId);
            Assert.Equal(dqtUser.DqtUserName, process.DqtUserName);
        });
    }

    [Fact]
    public async Task Execute_MoreEventsThanFitInOneBatch_MigratesThemAll()
    {
        // Arrange
        // The job's batch size is 5000; go a little over it so more than one batch is needed.
        const int eventCount = 5005;

        var person = await TestData.CreatePersonAsync();

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(i => new LegacyEvents.RouteToProfessionalStatusMigratedEvent
            {
                EventId = Guid.NewGuid(),
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                CreatedUtc = TimeProvider.UtcNow.AddSeconds(i),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                RouteToProfessionalStatus = CreateRoute(),
                PersonAttributes = CreatePersonAttributes(qtsDate: null),
                OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
                DqtInitialTeacherTraining = null,
                DqtQtsRegistration = null,
                DqtQtlsDate = null,
                DqtQtlsDateHasBeenSet = null
            })
            .ToArray();

        await WithDbContextAsync(async dbContext =>
        {
            foreach (var legacyEvent in legacyEvents)
            {
                dbContext.AddEventWithoutBroadcast(legacyEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var eventIds = legacyEvents.Select(e => e.EventId).ToArray();
            var migratedCount = await dbContext.ProcessEvents.CountAsync(pe => eventIds.Contains(pe.ProcessEventId));
            Assert.Equal(eventCount, migratedCount);
        });
    }

    [Fact]
    public async Task Execute_EventsWithTheSameCreatedTimestamp_MigratesThemAll()
    {
        // Arrange
        // The cursor is (created, event_id), so events sharing a timestamp shouldn't be skipped or repeated.
        const int eventCount = 50;

        var person = await TestData.CreatePersonAsync();
        var created = TimeProvider.UtcNow;

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(_ => new LegacyEvents.RouteToProfessionalStatusMigratedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = created,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                RouteToProfessionalStatus = CreateRoute(),
                PersonAttributes = CreatePersonAttributes(qtsDate: null),
                OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
                DqtInitialTeacherTraining = null,
                DqtQtsRegistration = null,
                DqtQtlsDate = null,
                DqtQtlsDateHasBeenSet = null
            })
            .ToArray();

        await WithDbContextAsync(async dbContext =>
        {
            foreach (var legacyEvent in legacyEvents)
            {
                dbContext.AddEventWithoutBroadcast(legacyEvent);
            }

            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var eventIds = legacyEvents.Select(e => e.EventId).ToArray();
            var migratedCount = await dbContext.ProcessEvents.CountAsync(pe => eventIds.Contains(pe.ProcessEventId));
            Assert.Equal(eventCount, migratedCount);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotBackfillTwice()
    {
        // Arrange
        var legacyEvent = await AddMigratedEventAsync();

        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.RouteToProfessionalStatusMigratingFromDqt)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var legacyEvent = await AddMigratedEventAsync();

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventOfAnotherType_IsNotBackfilled()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.DqtQtsRegistrationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            QtsRegistration = new EventModels.DqtQtsRegistration { TeacherStatusName = "Trainee teacher" }
        });

        // Act
        await WithServiceAsync<BackfillRouteToProfessionalStatusProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.RouteToProfessionalStatus CreateRoute(
        RouteToProfessionalStatusStatus status = RouteToProfessionalStatusStatus.InTraining) => new()
        {
            QualificationId = Guid.NewGuid(),
            RouteToProfessionalStatusTypeId = RouteToProfessionalStatusType.ApplyForQtsId,
            Status = status,
            HoldsFrom = null,
            TrainingStartDate = new DateOnly(2023, 9, 1),
            TrainingEndDate = new DateOnly(2024, 6, 30),
            TrainingSubjectIds = [],
            TrainingAgeSpecialismType = null,
            TrainingAgeSpecialismRangeFrom = null,
            TrainingAgeSpecialismRangeTo = null,
            TrainingCountryId = null,
            TrainingProviderId = null,
            DegreeTypeId = null,
            ExemptFromInduction = null,
            ExemptFromInductionDueToQtsDate = null,
            SourceApplicationUserId = null,
            SourceApplicationReference = null
        };

    private static EventModels.ProfessionalStatusPersonAttributes CreatePersonAttributes(DateOnly? qtsDate) => new()
    {
        QtsDate = qtsDate,
        EytsDate = null,
        HasEyps = false,
        PqtsDate = null,
        QtlsStatus = QtlsStatus.None
    };

    private static EventModels.Induction CreateInduction() => new()
    {
        Status = InductionStatus.None,
        StatusWithoutExemption = InductionStatus.None,
        StartDate = null,
        CompletedDate = null,
        ExemptionReasonIds = [],
        CpdCpdModifiedOn = Option.None<DateTime>(),
        InductionExemptWithoutReason = false
    };

    private async Task<LegacyEvents.RouteToProfessionalStatusMigratedEvent> AddMigratedEventAsync()
    {
        var person = await TestData.CreatePersonAsync();

        return await AddLegacyEventAsync(new LegacyEvents.RouteToProfessionalStatusMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = CreateRoute(),
            PersonAttributes = CreatePersonAttributes(qtsDate: null),
            OldPersonAttributes = CreatePersonAttributes(qtsDate: null),
            DqtInitialTeacherTraining = null,
            DqtQtsRegistration = null,
            DqtQtlsDate = null,
            DqtQtlsDateHasBeenSet = null
        });
    }

    private async Task<TEvent> AddLegacyEventAsync<TEvent>(TEvent legacyEvent) where TEvent : LegacyEvents.EventBase
    {
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        return legacyEvent;
    }
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillTrnRequestSupportTaskProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyTrnRequestTaskEvent_CreatesTrnRequestCreatingProcess()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var oneLoginUserSubject = Guid.NewGuid().ToString();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            applicationUser.UserId,
            SupportTaskType.TrnRequest,
            new TrnRequestData(),
            personId: null,
            oneLoginUserSubject);

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(SupportTaskCreatedEvent), processEvent.EventName);

            var createdEvent = Assert.IsType<SupportTaskCreatedEvent>(processEvent.Payload);
            Assert.Equal(legacyEvent.SupportTask.SupportTaskReference, createdEvent.SupportTask.SupportTaskReference);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.TrnRequestCreating, process.ProcessType);
            Assert.Equal(applicationUser.UserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(legacyEvent.SupportTask.SupportTaskReference, Assert.Single(process.SupportTaskReferences));
            Assert.Equal(oneLoginUserSubject, Assert.Single(process.OneLoginUserSubjects));
            Assert.Empty(process.PersonIds);

            Assert.Single(await dbContext.ProcessEvents.Where(pe => pe.ProcessId == process.ProcessId).ToListAsync());
        });
    }

    [Fact]
    public async Task Execute_LegacyManualChecksNeededTaskEventRaisedByApplicationUser_CreatesTrnRequestCreatingProcess()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            applicationUser.UserId,
            SupportTaskType.TrnRequestManualChecksNeeded,
            new TrnRequestManualChecksNeededData(),
            person.PersonId,
            oneLoginUserSubject: null);

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.TrnRequestCreating, process.ProcessType);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
        });
    }

    [Fact]
    public async Task Execute_LegacyManualChecksNeededTaskEventRaisedBySupportUser_CreatesTrnRequestResolvingProcess()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            user.UserId,
            SupportTaskType.TrnRequestManualChecksNeeded,
            new TrnRequestManualChecksNeededData(),
            person.PersonId,
            oneLoginUserSubject: null);

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.TrnRequestResolving, process.ProcessType);
            Assert.Equal(user.UserId, process.UserId);
        });
    }

    [Fact]
    public async Task Execute_LegacyNpqTrnRequestTaskEvent_CreatesNpqTrnRequestTaskCreatingProcess()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            applicationUser.UserId,
            SupportTaskType.NpqTrnRequest,
            new NpqTrnRequestData(),
            personId: null,
            oneLoginUserSubject: null);

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.NpqTrnRequestTaskCreating, process.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_LegacyTrnRequestTaskEventRaisedBySupportUser_Throws()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            user.UserId,
            SupportTaskType.TrnRequest,
            new TrnRequestData(),
            personId: null,
            oneLoginUserSubject: null);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
                job => job.ExecuteAsync(CancellationToken.None)));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(legacyEvent.SupportTask.SupportTaskReference, exception.Message);

        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventWithoutASupportTaskReference_Throws()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            applicationUser.UserId,
            SupportTaskType.TrnRequest,
            new TrnRequestData(),
            personId: null,
            oneLoginUserSubject: null,
            withoutReference: true);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
                job => job.ExecuteAsync(CancellationToken.None)));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(legacyEvent.EventId.ToString(), exception.Message);

        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyTeacherPensionsTaskEvent_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            ApplicationUser.CapitaTpsImportGuid,
            SupportTaskType.TeacherPensionsPotentialDuplicate,
            new TeacherPensionsPotentialDuplicateData { FileName = "tps.csv", IntegrationTransactionId = 1 },
            person.PersonId,
            oneLoginUserSubject: null);

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventThatAlreadyHasAProcess_IsLeftAlone()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var legacyEvent = await AddLegacyCreatedEventAsync(
            applicationUser.UserId,
            SupportTaskType.TrnRequest,
            new TrnRequestData(),
            personId: null,
            oneLoginUserSubject: null);

        // The dual-write era gave the legacy event and its process event the same id.
        await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            applicationUser.UserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = legacyEvent.EventId,
                SupportTask = legacyEvent.SupportTask
            });

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.TrnRequestCreating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotCreateDuplicateProcesses()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        await AddLegacyCreatedEventAsync(
            applicationUser.UserId,
            SupportTaskType.TrnRequest,
            new TrnRequestData(),
            personId: null,
            oneLoginUserSubject: null);

        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.TrnRequestCreating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_UnrelatedLegacyEvent_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = new LegacyEvents.PersonStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Status = PersonStatus.Deactivated,
            OldStatus = PersonStatus.Active,
            Reason = null,
            ReasonDetail = null,
            AdditionalInformation = null,
            EvidenceFile = null,
            DateOfDeath = null
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithServiceAsync<BackfillTrnRequestSupportTaskProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private async Task<LegacyEvents.SupportTaskCreatedEvent> AddLegacyCreatedEventAsync(
        Guid raisedByUserId,
        SupportTaskType supportTaskType,
        ISupportTaskData data,
        Guid? personId,
        string? oneLoginUserSubject,
        bool withoutReference = false)
    {
        var legacyEvent = new LegacyEvents.SupportTaskCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = raisedByUserId,
            SupportTask = new EventModels.SupportTask
            {
                SupportTaskReference = withoutReference ? null! : $"TEST-{Guid.NewGuid():N}"[..15],
                SupportTaskType = supportTaskType,
                Status = SupportTaskStatus.Open,
                OneLoginUserSubject = oneLoginUserSubject,
                PersonId = personId,
                Data = data,
                SourceApplicationUserId = null,
                ResolveJourneySavedState = null,
                AssignedToUserId = null,
                ZendeskTickets = [],
                Outcome = null
            }
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        return legacyEvent;
    }
}

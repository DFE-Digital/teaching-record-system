using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using Process = TeachingRecordSystem.Core.DataStore.Postgres.Models.Process;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillTeacherPensionsSupportTaskProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_AttachesEventToTheImportProcessThatCreatedThePerson()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        var legacyEvent = await AddLegacyCreatedEventAsync(supportTask);
        var processId = await AddImportProcessAsync(supportTask.PersonId!.Value, legacyEvent.CreatedUtc);

        // Act
        await WithServiceAsync<BackfillTeacherPensionsSupportTaskProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(processId, processEvent.ProcessId);
            Assert.Equal(nameof(SupportTaskCreatedEvent), processEvent.EventName);

            var createdEvent = Assert.IsType<SupportTaskCreatedEvent>(processEvent.Payload);
            Assert.Equal(supportTask.SupportTaskReference, createdEvent.SupportTask.SupportTaskReference);

            // No new process is created; the task reference is recorded against the existing one.
            var process = Assert.Single(await dbContext.Processes.Where(p => p.ProcessType == ProcessType.TeacherPensionsRecordImporting).ToListAsync());
            Assert.Equal(processId, process.ProcessId);
            Assert.Contains(supportTask.SupportTaskReference, process.SupportTaskReferences);
            Assert.Contains(supportTask.PersonId!.Value, process.PersonIds);
        });
    }

    [Fact]
    public async Task Execute_NoImportProcessExists_Throws()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        var legacyEvent = await AddLegacyCreatedEventAsync(supportTask);

        // Act
        var exception = await Record.ExceptionAsync(() =>
            WithServiceAsync<BackfillTeacherPensionsSupportTaskProcessesJob>(
                job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None)));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains(supportTask.SupportTaskReference, exception.Message);

        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotBackfillTwice()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        var legacyEvent = await AddLegacyCreatedEventAsync(supportTask);
        await AddImportProcessAsync(supportTask.PersonId!.Value, legacyEvent.CreatedUtc);

        await WithServiceAsync<BackfillTeacherPensionsSupportTaskProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillTeacherPensionsSupportTaskProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvents[0].ProcessId);
            Assert.Equal(supportTask.SupportTaskReference, Assert.Single(process.SupportTaskReferences));
        });
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        var legacyEvent = await AddLegacyCreatedEventAsync(supportTask);
        await AddImportProcessAsync(supportTask.PersonId!.Value, legacyEvent.CreatedUtc);

        // Act
        await WithServiceAsync<BackfillTeacherPensionsSupportTaskProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    [Fact]
    public async Task Execute_SupportTaskOfAnotherType_IsNotBackfilled()
    {
        // Arrange
        var result = await TestData.CreateTrnRequestSupportTaskAsync();
        var legacyEvent = await AddLegacyCreatedEventAsync(result.SupportTask);

        // Act
        await WithServiceAsync<BackfillTeacherPensionsSupportTaskProcessesJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private async Task<LegacyEvents.SupportTaskCreatedEvent> AddLegacyCreatedEventAsync(SupportTask supportTask)
    {
        var legacyEvent = new LegacyEvents.SupportTaskCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = supportTask.CreatedOn,
            RaisedBy = ApplicationUser.CapitaTpsImportUser.UserId,
            SupportTask = EventModels.SupportTask.FromModel(supportTask)
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(legacyEvent);
            await dbContext.SaveChangesAsync();
        });

        return legacyEvent;
    }

    private async Task<Guid> AddImportProcessAsync(Guid personId, DateTime createdOn)
    {
        var processId = Guid.NewGuid();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Processes.Add(new Process
            {
                ProcessId = processId,
                ProcessType = ProcessType.TeacherPensionsRecordImporting,
                CreatedOn = createdOn,
                UpdatedOn = createdOn,
                UserId = ApplicationUser.CapitaTpsImportUser.UserId,
                DqtUserId = null,
                DqtUserName = null,
                PersonIds = [personId],
                OneLoginUserSubjects = [],
                SupportTaskReferences = [],
                ChangeReason = null
            });

            await dbContext.SaveChangesAsync();
        });

        return processId;
    }
}

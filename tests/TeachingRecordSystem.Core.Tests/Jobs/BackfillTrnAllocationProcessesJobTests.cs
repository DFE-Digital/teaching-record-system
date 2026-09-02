using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillTrnAllocationProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyTrnAllocatedEvent_CreatesProcessWithTrnAllocatedEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var legacyEvent = await AddLegacyTrnAllocatedEventAsync(person);

        // Act
        await WithServiceAsync<BackfillTrnAllocationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents
                .SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(TrnAllocatedEvent), processEvent.EventName);

            var trnAllocatedEvent = Assert.IsType<TrnAllocatedEvent>(processEvent.Payload);
            Assert.Equal(person.PersonId, trnAllocatedEvent.PersonId);
            Assert.Equal(person.Trn, trnAllocatedEvent.Trn);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.TrnAllocating, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventThatAlreadyHasAProcess_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var legacyEvent = await AddLegacyTrnAllocatedEventAsync(person);

        await TestData.CreateProcessAsync(
            ProcessType.TrnAllocating,
            SystemUser.SystemUserId,
            changeReason: null,
            new TrnAllocatedEvent
            {
                EventId = legacyEvent.EventId,
                PersonId = person.PersonId,
                Trn = person.Trn!
            });

        // Act
        await WithServiceAsync<BackfillTrnAllocationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.TrnAllocating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_RunTwice_DoesNotCreateDuplicateProcesses()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await AddLegacyTrnAllocatedEventAsync(person);

        await WithServiceAsync<BackfillTrnAllocationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillTrnAllocationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.TrnAllocating)
                .ToListAsync();
            Assert.Single(processes);
        });
    }

    [Fact]
    public async Task Execute_UnrelatedLegacyEvent_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await AddLegacyEventAsync(new LegacyEvents.PersonStatusUpdatedEvent
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
        });

        // Act
        await WithServiceAsync<BackfillTrnAllocationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processes = await dbContext.Processes
                .Where(p => p.PersonIds.Contains(person.PersonId) && p.ProcessType == ProcessType.TrnAllocating)
                .ToListAsync();
            Assert.Empty(processes);
        });
    }

    private Task<LegacyEvents.TrnAllocatedEvent> AddLegacyTrnAllocatedEventAsync(Person person) =>
        AddLegacyEventAsync(new LegacyEvents.TrnAllocatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Trn = person.Trn!
        });

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

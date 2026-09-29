using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillTpsEmploymentProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyCreatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var employment = CreateTpsEmployment(person.PersonId);

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.TpsEmploymentCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            TpsEmployment = employment
        });

        // Act
        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(TpsEmploymentCreatedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));
            Assert.Equal(legacyEvent.CreatedUtc, processEvent.CreatedOn);

            var createdEvent = Assert.IsType<TpsEmploymentCreatedEvent>(processEvent.Payload);
            Assert.Equal(legacyEvent.EventId, createdEvent.EventId);
            Assert.Equal(person.PersonId, createdEvent.PersonId);
            AssertTpsEmploymentEqual(employment, createdEvent.TpsEmployment);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.TpsEmploymentCreating, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Null(process.DqtUserId);
            Assert.Null(process.DqtUserName);
            Assert.Equal(legacyEvent.CreatedUtc, process.CreatedOn);
            Assert.Equal(legacyEvent.CreatedUtc, process.UpdatedOn);
            Assert.Equal(person.PersonId, Assert.Single(process.PersonIds));
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_LegacyUpdatedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var employment = CreateTpsEmployment(person.PersonId);
        var oldEmployment = employment with { EndDate = null, EmployerEmailAddress = "old@example.com" };

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.TpsEmploymentUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            TpsEmployment = employment,
            OldTpsEmployment = oldEmployment,
            Changes = LegacyEvents.TpsEmploymentUpdatedEventChanges.EndDate |
                LegacyEvents.TpsEmploymentUpdatedEventChanges.EmployerEmailAddress
        });

        // Act
        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(TpsEmploymentUpdatedEvent), processEvent.EventName);

            var updatedEvent = Assert.IsType<TpsEmploymentUpdatedEvent>(processEvent.Payload);
            Assert.Equal(legacyEvent.EventId, updatedEvent.EventId);
            Assert.Equal(person.PersonId, updatedEvent.PersonId);
            AssertTpsEmploymentEqual(employment, updatedEvent.TpsEmployment);
            AssertTpsEmploymentEqual(oldEmployment, updatedEvent.OldTpsEmployment);
            Assert.Equal(
                TpsEmploymentUpdatedEventChanges.EndDate | TpsEmploymentUpdatedEventChanges.EmployerEmailAddress,
                updatedEvent.Changes);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.TpsEmploymentUpdating, process.ProcessType);
            Assert.Equal(SystemUser.SystemUserId, process.UserId);
            Assert.Null(process.ChangeReason);
        });
    }

    [Fact]
    public async Task Execute_MoreEventsThanFitInOneBatch_MigratesThemAll()
    {
        // Arrange
        // The job's batch size is 5000; go a little over it so more than one batch is needed.
        const int eventCount = 5005;

        var person = await TestData.CreatePersonAsync();
        var employment = CreateTpsEmployment(person.PersonId);

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(i => new LegacyEvents.TpsEmploymentCreatedEvent
            {
                EventId = Guid.NewGuid(),
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                CreatedUtc = TimeProvider.UtcNow.AddSeconds(i),
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                TpsEmployment = employment
            })
            .ToArray();

        await AddLegacyEventsAsync(legacyEvents);

        // Act
        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

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
        var employment = CreateTpsEmployment(person.PersonId);
        var created = TimeProvider.UtcNow;

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(_ => new LegacyEvents.TpsEmploymentCreatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = created,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                TpsEmployment = employment
            })
            .ToArray();

        await AddLegacyEventsAsync(legacyEvents);

        // Act
        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

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
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(new LegacyEvents.TpsEmploymentCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            TpsEmployment = CreateTpsEmployment(person.PersonId)
        });

        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.TpsEmploymentCreating && p.PersonIds.Contains(person.PersonId))
                .ToListAsync();
            Assert.Single(processes);
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
        await WithServiceAsync<BackfillTpsEmploymentProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.TpsEmployment CreateTpsEmployment(Guid personId) => new()
    {
        PersonEmploymentId = Guid.NewGuid(),
        PersonId = personId,
        EstablishmentId = Guid.NewGuid(),
        StartDate = new DateOnly(2024, 9, 1),
        EndDate = new DateOnly(2025, 7, 31),
        LastKnownTpsEmployedDate = new DateOnly(2025, 7, 31),
        EmploymentType = EmploymentType.FullTime,
        WithdrawalConfirmed = false,
        LastExtractDate = new DateOnly(2025, 8, 31),
        NationalInsuranceNumber = "AB123456C",
        PersonPostcode = "AB1 2CD",
        PersonEmailAddress = "teacher@example.com",
        EmployerPostcode = "EF3 4GH",
        EmployerEmailAddress = "school@example.com",
        Key = "the-key"
    };

    private static void AssertTpsEmploymentEqual(EventModels.TpsEmployment expected, EventModels.TpsEmployment actual)
    {
        Assert.Equal(expected.PersonEmploymentId, actual.PersonEmploymentId);
        Assert.Equal(expected.PersonId, actual.PersonId);
        Assert.Equal(expected.EstablishmentId, actual.EstablishmentId);
        Assert.Equal(expected.StartDate, actual.StartDate);
        Assert.Equal(expected.EndDate, actual.EndDate);
        Assert.Equal(expected.LastKnownTpsEmployedDate, actual.LastKnownTpsEmployedDate);
        Assert.Equal(expected.EmploymentType, actual.EmploymentType);
        Assert.Equal(expected.WithdrawalConfirmed, actual.WithdrawalConfirmed);
        Assert.Equal(expected.LastExtractDate, actual.LastExtractDate);
        Assert.Equal(expected.NationalInsuranceNumber, actual.NationalInsuranceNumber);
        Assert.Equal(expected.PersonPostcode, actual.PersonPostcode);
        Assert.Equal(expected.PersonEmailAddress, actual.PersonEmailAddress);
        Assert.Equal(expected.EmployerPostcode, actual.EmployerPostcode);
        Assert.Equal(expected.EmployerEmailAddress, actual.EmployerEmailAddress);
        Assert.Equal(expected.Key, actual.Key);
    }

    private async Task<TEvent> AddLegacyEventAsync<TEvent>(TEvent legacyEvent) where TEvent : LegacyEvents.EventBase
    {
        await AddLegacyEventsAsync([legacyEvent]);
        return legacyEvent;
    }

    private Task AddLegacyEventsAsync(IEnumerable<LegacyEvents.EventBase> legacyEvents) =>
        WithDbContextAsync(async dbContext =>
        {
            foreach (var legacyEvent in legacyEvents)
            {
                dbContext.AddEventWithoutBroadcast(legacyEvent);
            }

            await dbContext.SaveChangesAsync();
        });
}

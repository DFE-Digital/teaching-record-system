using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillPersonMigrationProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_LegacyMigratedEvent_CreatesProcessAndProcessEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var attributes = CreatePersonAttributes();

        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(person.PersonId, person.Trn, attributes));

        // Act
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.NotNull(processEvent);
            Assert.Equal(nameof(PersonMigratedEvent), processEvent.EventName);
            Assert.Equal(person.PersonId, Assert.Single(processEvent.PersonIds));
            Assert.Equal(legacyEvent.CreatedUtc, processEvent.CreatedOn);

            var migratedEvent = Assert.IsType<PersonMigratedEvent>(processEvent.Payload);
            // The legacy payload has no EventId of its own; it comes from the events row.
            Assert.Equal(legacyEvent.EventId, migratedEvent.EventId);
            Assert.Equal(person.PersonId, migratedEvent.PersonId);
            Assert.Equal(person.Trn, migratedEvent.Trn);
            Assert.Equal(attributes.FirstName, migratedEvent.PersonAttributes.FirstName);
            Assert.Equal(attributes.MiddleName, migratedEvent.PersonAttributes.MiddleName);
            Assert.Equal(attributes.LastName, migratedEvent.PersonAttributes.LastName);
            Assert.Equal(attributes.DateOfBirth, migratedEvent.PersonAttributes.DateOfBirth);
            Assert.Equal(attributes.EmailAddress, migratedEvent.PersonAttributes.EmailAddress);
            Assert.Equal(attributes.NationalInsuranceNumber, migratedEvent.PersonAttributes.NationalInsuranceNumber);
            Assert.Equal(attributes.Gender, migratedEvent.PersonAttributes.Gender);

            var process = await dbContext.Processes.SingleAsync(p => p.ProcessId == processEvent.ProcessId);
            Assert.Equal(ProcessType.PersonMigratingFromDqt, process.ProcessType);
            // The legacy payload has no RaisedBy - the job that wrote these ran unattended.
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
    public async Task Execute_LegacyEventWithPlaceholderTrn_RecoversTheTrnFromThePerson()
    {
        // Arrange
        // The first version of CreatePersonMigratedEventsJob wrote the literal string 'trn' in place of the TRN.
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(person.PersonId, "trn", CreatePersonAttributes()));

        // Act
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var migratedEvent = Assert.IsType<PersonMigratedEvent>(processEvent.Payload);

            Assert.Equal(person.Trn, migratedEvent.Trn);
        });
    }

    [Fact]
    public async Task Execute_LegacyEventWithNoTrn_LeavesTheTrnNull()
    {
        // Arrange
        // A genuinely absent TRN is carried over untouched even though the person has one: only the exact
        // 'trn' placeholder is repaired.
        var person = await TestData.CreatePersonAsync();

        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(person.PersonId, null, CreatePersonAttributes()));

        // Act
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            var migratedEvent = Assert.IsType<PersonMigratedEvent>(processEvent.Payload);

            Assert.Null(migratedEvent.Trn);
        });
    }

    [Fact]
    public async Task Execute_MoreEventsThanFitInOneBatch_MigratesThemAll()
    {
        // Arrange
        // The job's batch size is 5000; go a little over it so more than one batch is needed.
        const int eventCount = 5005;

        var person = await TestData.CreatePersonAsync();
        var attributes = CreatePersonAttributes();

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(i => CreateLegacyEvent(
                person.PersonId,
                person.Trn,
                attributes,
                // Distinct timestamps so the (created, event_id) paging has a stable order to walk.
                createdUtc: TimeProvider.UtcNow.AddSeconds(i)))
            .ToArray();

        await AddLegacyEventsAsync(legacyEvents);

        // Act
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
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
        var attributes = CreatePersonAttributes();
        var created = TimeProvider.UtcNow;

        var legacyEvents = Enumerable.Range(0, eventCount)
            .Select(_ => CreateLegacyEvent(person.PersonId, person.Trn, attributes, createdUtc: created))
            .ToArray();

        await AddLegacyEventsAsync(legacyEvents);

        // Act
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
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
        var legacyEvent = await AddLegacyEventAsync(CreateLegacyEvent(person.PersonId, person.Trn, CreatePersonAttributes()));

        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvents = await dbContext.ProcessEvents.Where(pe => pe.ProcessEventId == legacyEvent.EventId).ToListAsync();
            Assert.Single(processEvents);

            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.PersonMigratingFromDqt && p.PersonIds.Contains(person.PersonId))
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
        await WithServiceAsync<BackfillPersonMigrationProcessesJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleOrDefaultAsync(pe => pe.ProcessEventId == legacyEvent.EventId);
            Assert.Null(processEvent);
        });
    }

    private static EventModels.PersonDetails CreatePersonAttributes() => new()
    {
        FirstName = "Alfred",
        MiddleName = "The",
        LastName = "Great",
        DateOfBirth = new DateOnly(1980, 1, 1),
        EmailAddress = "alfred@example.com",
        NationalInsuranceNumber = "AB123456C",
        Gender = Gender.Male
    };

    private LegacyEvents.PersonMigratedEvent CreateLegacyEvent(
        Guid personId,
        string? trn,
        EventModels.PersonDetails attributes,
        DateTime? createdUtc = null) =>
        new()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = createdUtc ?? TimeProvider.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = personId,
            Trn = trn,
            PersonAttributes = attributes
        };

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

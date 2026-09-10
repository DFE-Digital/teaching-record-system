using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class RepairOverseasNpqPersonCreationProcessesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_MisTypedOverseasNpqProcess_BecomesPersonCreating()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var process = await CreateMisTypedProcessAsync(person);

        // Act
        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var repaired = await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
            Assert.Equal(ProcessType.PersonCreating, repaired.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_MisTypedProcessWithItsBackfilledEmail_KeepsTheEmailOnTheProcess()
    {
        // Arrange
        // The two jobs are order-independent; re-typing must not disturb an email already attached.
        var person = await TestData.CreatePersonAsync();
        var process = await CreateMisTypedProcessAsync(person);
        await AddEmailSentEventAsync(process, person);

        // Act
        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var repaired = await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
            Assert.Equal(ProcessType.PersonCreating, repaired.ProcessType);

            var processEvents = await dbContext.ProcessEvents
                .Where(pe => pe.ProcessId == process.ProcessId)
                .ToListAsync();
            Assert.Contains(processEvents, pe => pe.EventName == nameof(EmailSentEvent));
        });
    }

    [Fact]
    public async Task Execute_RunTwice_LeavesTheRepairedProcessAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var process = await CreateMisTypedProcessAsync(person);

        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Act
        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var repaired = await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
            Assert.Equal(ProcessType.PersonCreating, repaired.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_GenuineTeacherPensionsImport_IsLeftAlone()
    {
        // Arrange
        // A real import runs as the Capita Teachers' Pensions user and stamps the person as created by TPS.
        var person = await TestData.CreatePersonAsync(p => p.WithCreatedByTps(true));

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsRecordImporting,
            ApplicationUser.CapitaTpsImportGuid,
            changeReason: null,
            CreatePersonCreatedEvent(person));

        // Act
        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var unchanged = await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
            Assert.Equal(ProcessType.TeacherPensionsRecordImporting, unchanged.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_SystemUserProcessForAPersonCreatedByTps_IsLeftAlone()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithCreatedByTps(true));
        var process = await CreateMisTypedProcessAsync(person);

        // Act
        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var unchanged = await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
            Assert.Equal(ProcessType.TeacherPensionsRecordImporting, unchanged.ProcessType);
        });
    }

    [Fact]
    public async Task Execute_ProcessThatAlsoRaisedADuplicateTask_IsLeftAlone()
    {
        // Arrange
        // An import that flagged a duplicate holds a second event, so it isn't from the bulk run.
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        var person = await WithDbContextAsync(dbContext =>
            dbContext.Persons.SingleAsync(p => p.PersonId == supportTask.PersonId!.Value));

        var process = await TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsRecordImporting,
            SystemUser.SystemUserId,
            changeReason: null,
            CreatePersonCreatedEvent(person),
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        // Act
        await WithServiceAsync<RepairOverseasNpqPersonCreationProcessesJob>(job => job.ExecuteAsync(CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var unchanged = await dbContext.Processes.SingleAsync(p => p.ProcessId == process.ProcessId);
            Assert.Equal(ProcessType.TeacherPensionsRecordImporting, unchanged.ProcessType);
        });
    }

    private Task<Process> CreateMisTypedProcessAsync(Person person) =>
        TestData.CreateProcessAsync(
            ProcessType.TeacherPensionsRecordImporting,
            SystemUser.SystemUserId,
            changeReason: null,
            CreatePersonCreatedEvent(person));

    private static PersonCreatedEvent CreatePersonCreatedEvent(Person person) => new()
    {
        EventId = Guid.NewGuid(),
        PersonId = person.PersonId,
        Details = EventModels.PersonDetails.FromModel(person),
        TrnRequestMetadata = null
    };

    private async Task AddEmailSentEventAsync(Process process, Person person)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.TrnGeneratedForNpq,
            EmailAddress = TestData.GenerateUniqueEmail(),
            Personalization = new Dictionary<string, string> { { "trn", person.Trn! } },
            SentOn = TimeProvider.UtcNow
        };

        IEvent emailSentEvent = new EmailSentEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            Email = EventModels.Email.FromModel(email)
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Emails.Add(email);

            dbContext.ProcessEvents.Add(new ProcessEvent
            {
                ProcessEventId = emailSentEvent.EventId,
                ProcessId = process.ProcessId,
                EventName = emailSentEvent.GetType().Name,
                Payload = emailSentEvent,
                PersonIds = emailSentEvent.PersonIds,
                OneLoginUserSubjects = emailSentEvent.OneLoginUserSubjects,
                SupportTaskReferences = emailSentEvent.SupportTaskReferences,
                CreatedOn = email.SentOn!.Value
            });

            await dbContext.SaveChangesAsync();
        });
    }
}

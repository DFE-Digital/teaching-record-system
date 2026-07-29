using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillSupportTaskSourceApplicationUserJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task ExecuteAsync_TrnRequest_SetsSourceFromTrnRequestApplicationUser()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var result = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(result.SupportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(result.SupportTask));
    }

    [Fact]
    public async Task ExecuteAsync_NpqTrnRequest_SetsSourceFromRelatedTrnRequestApplicationUser()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await CreateNpqTrnRequestTaskAsync(applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_TrnRequestManualChecksNeeded_SetsSourceFromTrnRequestApplicationUser()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_TeacherPensionsPotentialDuplicate_SetsSourceFromTrnRequestApplicationUser()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_OneLoginUserRecordMatching_SetsSourceFromClientApplicationUserInData()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUser.UserId));

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_OneLoginUserIdVerification_SetsSourceFromClientApplicationUserInData()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUser.UserId));

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ChangeNameRequestWithCreatingProcess_SetsSourceFromProcessUser()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        await CreateCreatingProcessAsync(supportTask, ProcessType.ChangeOfNameRequestCreating, applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ChangeDateOfBirthRequestWithCreatingProcess_SetsSourceFromProcessUser()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync();

        await CreateCreatingProcessAsync(supportTask, ProcessType.ChangeOfDateOfBirthRequestCreating, applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ChangeRequestWithOnlyLegacyCreatedEvent_SetsSourceFromEventRaisedBy()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(new LegacyEvents.SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = applicationUser.UserId,
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

            await dbContext.SaveChangesAsync();
        });

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(applicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ChangeRequestWithNoProcessOrEvent_IsLeftNull()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Null(await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ChangeRequestCreatedByStaffUser_IsLeftNull()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        await CreateCreatingProcessAsync(supportTask, ProcessType.ChangeOfNameRequestCreating, user.UserId);
        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Null(await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_TaskThatAlreadyHasASource_IsLeftUnchanged()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUser.UserId));

        // Point the stored value at a different application user so a re-derivation would be visible.
        var existingApplicationUser = await TestData.CreateApplicationUserAsync();
        await SetSourceApplicationUserIdAsync(supportTask, existingApplicationUser.UserId);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(existingApplicationUser.UserId, await GetSourceApplicationUserIdAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_DryRun_DoesNotPersistChanges()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var result = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);
        await ClearSourceApplicationUserIdAsync(result.SupportTask);

        // Act
        await ExecuteJobAsync(dryRun: true);

        // Assert
        Assert.Null(await GetSourceApplicationUserIdAsync(result.SupportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ProcessEventCarryingTask_SetsSourceOnPayload()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var result = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);
        var supportTask = result.SupportTask;

        var eventId = Guid.NewGuid();

        await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            SystemUser.SystemUserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = eventId,
                SupportTask = EventModels.SupportTask.FromModel(supportTask) with { SourceApplicationUserId = null }
            });

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        var payload = await GetProcessEventPayloadAsync<SupportTaskCreatedEvent>(eventId);
        Assert.Equal(applicationUser.UserId, payload.SupportTask.SourceApplicationUserId);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessEventWithOldAndNewTask_SetsSourceOnBoth()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var result = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);
        var supportTask = result.SupportTask;

        var eventId = Guid.NewGuid();
        var eventSupportTask = EventModels.SupportTask.FromModel(supportTask) with { SourceApplicationUserId = null };

        await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            SystemUser.SystemUserId,
            changeReason: null,
            new SupportTaskUpdatedEvent
            {
                EventId = eventId,
                SupportTaskReference = supportTask.SupportTaskReference,
                Changes = SupportTaskUpdatedEventChanges.Status,
                SupportTask = eventSupportTask with { Status = SupportTaskStatus.Closed },
                OldSupportTask = eventSupportTask,
                Comments = null,
                RejectionReason = null
            });

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        var payload = await GetProcessEventPayloadAsync<SupportTaskUpdatedEvent>(eventId);
        Assert.Equal(applicationUser.UserId, payload.SupportTask.SourceApplicationUserId);
        Assert.Equal(applicationUser.UserId, payload.OldSupportTask.SourceApplicationUserId);
    }

    [Fact]
    public async Task ExecuteAsync_LegacyEventCarryingTask_SetsSourceOnPayload()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var person = await TestData.CreatePersonAsync();
        var eventId = Guid.NewGuid();

        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);
        await CreateCreatingProcessAsync(supportTask, ProcessType.ChangeOfNameRequestCreating, applicationUser.UserId);

        var eventSupportTask = EventModels.SupportTask.FromModel(supportTask) with { SourceApplicationUserId = null };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(new LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent
            {
                EventId = eventId,
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                RequestData = EventModels.ChangeNameRequestData.FromModel(supportTask.GetData<ChangeNameRequestData>()),
                RejectionReason = null,
                SupportTask = eventSupportTask with { Status = SupportTaskStatus.Closed },
                OldSupportTask = eventSupportTask
            });

            await dbContext.SaveChangesAsync();
        });

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var @event = await dbContext.Events.SingleAsync(e => e.EventId == eventId);
            var eventBase = Assert.IsType<LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent>(@event.ToEventBase());

            Assert.Equal(applicationUser.UserId, eventBase.SupportTask.SourceApplicationUserId);
            Assert.Equal(applicationUser.UserId, eventBase.OldSupportTask.SourceApplicationUserId);
        });
    }

    [Fact]
    public async Task ExecuteAsync_EventForTaskThatAlreadyHadASource_IsStillFixedUp()
    {
        // Arrange — the task keeps its column; only the event pre-dates the payload carrying it.
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var result = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);
        var supportTask = result.SupportTask;

        var eventId = Guid.NewGuid();

        await TestData.CreateProcessAsync(
            ProcessType.TrnRequestCreating,
            SystemUser.SystemUserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = eventId,
                SupportTask = EventModels.SupportTask.FromModel(supportTask) with { SourceApplicationUserId = null }
            });

        // Act
        await ExecuteJobAsync();

        // Assert
        var payload = await GetProcessEventPayloadAsync<SupportTaskCreatedEvent>(eventId);
        Assert.Equal(applicationUser.UserId, payload.SupportTask.SourceApplicationUserId);
    }

    [Fact]
    public async Task ExecuteAsync_EventForUnresolvableTask_IsLeftUnchanged()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();
        var eventId = Guid.NewGuid();

        await TestData.CreateProcessAsync(
            ProcessType.ChangeOfNameRequestCreating,
            SystemUser.SystemUserId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = eventId,
                SupportTask = EventModels.SupportTask.FromModel(supportTask) with { SourceApplicationUserId = null }
            });

        await ClearSourceApplicationUserIdAsync(supportTask);

        // Act
        await ExecuteJobAsync();

        // Assert
        var payload = await GetProcessEventPayloadAsync<SupportTaskCreatedEvent>(eventId);
        Assert.Null(payload.SupportTask.SourceApplicationUserId);
    }

    private Task<T> GetProcessEventPayloadAsync<T>(Guid eventId) where T : IEvent =>
        WithDbContextAsync(async dbContext =>
        {
            var processEvent = await dbContext.ProcessEvents.SingleAsync(e => e.ProcessEventId == eventId);
            return Assert.IsType<T>(processEvent.Payload);
        });

    private Task ExecuteJobAsync(bool dryRun = false) =>
        WithDbContextAsync(dbContext => new BackfillSupportTaskSourceApplicationUserJob(
                dbContext,
                NullLogger<BackfillSupportTaskSourceApplicationUserJob>.Instance)
            .ExecuteAsync(dryRun, CancellationToken.None));

    private Task ClearSourceApplicationUserIdAsync(SupportTask supportTask) =>
        SetSourceApplicationUserIdAsync(supportTask, null);

    private Task SetSourceApplicationUserIdAsync(SupportTask supportTask, Guid? sourceApplicationUserId) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks
                .IgnoreQueryFilters()
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);

            // The property is init-only, so go through the entry rather than the model.
            dbContext.Entry(dbSupportTask).Property(t => t.SourceApplicationUserId).CurrentValue = sourceApplicationUserId;

            await dbContext.SaveChangesAsync();
        });

    private Task<Guid?> GetSourceApplicationUserIdAsync(SupportTask supportTask) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks
                .IgnoreQueryFilters()
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);

            return dbSupportTask.SourceApplicationUserId;
        });

    private Task CreateCreatingProcessAsync(SupportTask supportTask, ProcessType processType, Guid userId) =>
        TestData.CreateProcessAsync(
            processType,
            userId,
            changeReason: null,
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

    private async Task<SupportTask> CreateNpqTrnRequestTaskAsync(Guid applicationUserId)
    {
        var person = await TestData.CreatePersonAsync();

        var metadata = new TrnRequestMetadata
        {
            ApplicationUserId = applicationUserId,
            RequestId = Guid.NewGuid().ToString(),
            CreatedOn = TimeProvider.UtcNow,
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Name = [person.FirstName, person.LastName],
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = person.EmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            PotentialDuplicate = false
        };

        var subject = SupportTask.Subject.FromTrnRequest(metadata);

        var supportTask = new SupportTask
        {
            CreatedOn = TimeProvider.UtcNow,
            UpdatedOn = TimeProvider.UtcNow,
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            Status = SupportTaskStatus.Open,
            Data = new NpqTrnRequestData(),
            PersonId = person.PersonId,
            TrnRequestApplicationUserId = applicationUserId,
            TrnRequestId = metadata.RequestId,
            SubjectName = subject.Name,
            SubjectEmailAddress = subject.EmailAddress,
            SourceApplicationUserId = applicationUserId
        };

        return await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(metadata);
            dbContext.SupportTasks.Add(supportTask);
            await dbContext.SaveChangesAsync();
            return supportTask;
        });
    }
}

using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillSupportTaskOutcomeJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Theory]
    [InlineData(SupportRequestOutcome.Approved, SupportTaskOutcome.ChangeNameRequest_Approved)]
    [InlineData(SupportRequestOutcome.Rejected, SupportTaskOutcome.ChangeNameRequest_Rejected)]
    [InlineData(SupportRequestOutcome.Cancelled, SupportTaskOutcome.ChangeNameRequest_Cancelled)]
    public async Task ExecuteAsync_ClosedChangeNameRequest_SetsOutcomeFromChangeRequestOutcome(
        SupportRequestOutcome changeRequestOutcome,
        SupportTaskOutcome expectedOutcome)
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(t => t.WithStatus(SupportTaskStatus.Closed));
        await SetDataAsync(supportTask, supportTask.GetData<ChangeNameRequestData>() with { ChangeRequestOutcome = changeRequestOutcome });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(expectedOutcome, await GetOutcomeAsync(supportTask));
    }

    [Theory]
    [InlineData(SupportRequestOutcome.Approved, SupportTaskOutcome.ChangeDateOfBirthRequest_Approved)]
    [InlineData(SupportRequestOutcome.Rejected, SupportTaskOutcome.ChangeDateOfBirthRequest_Rejected)]
    [InlineData(SupportRequestOutcome.Cancelled, SupportTaskOutcome.ChangeDateOfBirthRequest_Cancelled)]
    public async Task ExecuteAsync_ClosedChangeDateOfBirthRequest_SetsOutcomeFromChangeRequestOutcome(
        SupportRequestOutcome changeRequestOutcome,
        SupportTaskOutcome expectedOutcome)
    {
        // Arrange
        var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(t => t.WithStatus(SupportTaskStatus.Closed));
        await SetDataAsync(supportTask, supportTask.GetData<ChangeDateOfBirthRequestData>() with { ChangeRequestOutcome = changeRequestOutcome });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(expectedOutcome, await GetOutcomeAsync(supportTask));
    }

    [Theory]
    [InlineData(OneLoginUserIdVerificationOutcome.NotVerified, SupportTaskOutcome.OneLoginUserIdVerification_NotVerified)]
    [InlineData(OneLoginUserIdVerificationOutcome.VerifiedOnlyWithMatches, SupportTaskOutcome.OneLoginUserIdVerification_VerifiedOnlyWithMatches)]
    [InlineData(OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches, SupportTaskOutcome.OneLoginUserIdVerification_VerifiedOnlyWithoutMatches)]
    [InlineData(OneLoginUserIdVerificationOutcome.VerifiedAndConnected, SupportTaskOutcome.OneLoginUserIdVerification_VerifiedAndConnected)]
    public async Task ExecuteAsync_ClosedOneLoginUserIdVerification_SetsOutcomeFromDataOutcome(
        OneLoginUserIdVerificationOutcome dataOutcome,
        SupportTaskOutcome expectedOutcome)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithStatus(SupportTaskStatus.Closed));

        await SetDataAsync(supportTask, supportTask.GetData<OneLoginUserIdVerificationData>() with { Outcome = dataOutcome });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(expectedOutcome, await GetOutcomeAsync(supportTask));
    }

    [Theory]
    [InlineData(OneLoginUserRecordMatchingOutcome.NotConnecting, SupportTaskOutcome.OneLoginUserRecordMatching_NotConnecting)]
    [InlineData(OneLoginUserRecordMatchingOutcome.NoMatches, SupportTaskOutcome.OneLoginUserRecordMatching_NoMatches)]
    [InlineData(OneLoginUserRecordMatchingOutcome.Connected, SupportTaskOutcome.OneLoginUserRecordMatching_Connected)]
    public async Task ExecuteAsync_ClosedOneLoginUserRecordMatching_SetsOutcomeFromDataOutcome(
        OneLoginUserRecordMatchingOutcome dataOutcome,
        SupportTaskOutcome expectedOutcome)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithStatus(SupportTaskStatus.Closed));

        await SetDataAsync(supportTask, supportTask.GetData<OneLoginUserRecordMatchingData>() with { Outcome = dataOutcome });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(expectedOutcome, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTrnRequestWithSelectedPersonAttributes_SetsOutcomeToResolvedWithExistingPerson()
    {
        // Arrange
        // A snapshot of the selected record is only taken when resolving against an existing one.
        var supportTask = await CreateClosedTrnRequestTaskAsync();
        await SetDataAsync(supportTask, new TrnRequestData { SelectedPersonAttributes = CreateTrnRequestAttributes() });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.TrnRequest_ResolvedWithExistingPerson, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTrnRequestWithoutSelectedPersonAttributes_SetsOutcomeToResolvedWithNewPerson()
    {
        // Arrange
        var supportTask = await CreateClosedTrnRequestTaskAsync();
        await SetDataAsync(supportTask, new TrnRequestData { SelectedPersonAttributes = null });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.TrnRequest_ResolvedWithNewPerson, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTrnRequestManualChecksNeeded_SetsOutcomeToCompleted()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(status: SupportTaskStatus.Closed);

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.TrnRequestManualChecksNeeded_Completed, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTeacherPensionsTaskWithSelectedPersonAttributes_SetsOutcomeToResolvedWithMerge()
    {
        // Arrange
        // A snapshot of the record kept is only taken when the records were merged.
        var supportTask = await CreateClosedTeacherPensionsTaskAsync();
        await SetDataAsync(
            supportTask,
            supportTask.GetData<TeacherPensionsPotentialDuplicateData>() with
            {
                SelectedPersonAttributes = CreateTeacherPensionsAttributes()
            });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithMerge, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTeacherPensionsTaskWithoutSelectedPersonAttributes_SetsOutcomeToResolvedWithoutMerge()
    {
        // Arrange
        var supportTask = await CreateClosedTeacherPensionsTaskAsync();

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.TeacherPensionsPotentialDuplicate_ResolvedWithoutMerge, await GetOutcomeAsync(supportTask));
    }

    // The NPQ journey's UI is gone, but the tasks it left behind still need an outcome.
    [Fact]
    public async Task ExecuteAsync_ClosedApprovedNpqTrnRequestWithSelectedPersonAttributes_SetsOutcomeToResolvedWithExistingPerson()
    {
        // Arrange
        var supportTask = await CreateClosedNpqTrnRequestTaskAsync(new NpqTrnRequestData
        {
            SupportRequestOutcome = SupportRequestOutcome.Approved,
            SelectedPersonAttributes = CreateNpqAttributes()
        });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.NpqTrnRequest_ResolvedWithExistingPerson, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedApprovedNpqTrnRequestWithoutSelectedPersonAttributes_SetsOutcomeToResolvedWithNewPerson()
    {
        // Arrange
        var supportTask = await CreateClosedNpqTrnRequestTaskAsync(new NpqTrnRequestData
        {
            SupportRequestOutcome = SupportRequestOutcome.Approved,
            SelectedPersonAttributes = null
        });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.NpqTrnRequest_ResolvedWithNewPerson, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedRejectedNpqTrnRequest_SetsOutcomeToRejected()
    {
        // Arrange
        var supportTask = await CreateClosedNpqTrnRequestTaskAsync(new NpqTrnRequestData
        {
            SupportRequestOutcome = SupportRequestOutcome.Rejected
        });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.NpqTrnRequest_Rejected, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_OpenTask_LeavesOutcomeUnset()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(t => t.WithStatus(SupportTaskStatus.Open));

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Null(await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_TaskWithOutcomeAlreadySet_LeavesOutcomeUnchanged()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(t => t.WithStatus(SupportTaskStatus.Closed));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);
            supportTask.Data = supportTask.GetData<ChangeNameRequestData>() with { ChangeRequestOutcome = SupportRequestOutcome.Approved };
            // The data says Approved, so a task the job touched would come out Approved rather than Rejected.
            supportTask.Outcome = SupportTaskOutcome.ChangeNameRequest_Rejected;
            await dbContext.SaveChangesAsync();
        });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.ChangeNameRequest_Rejected, await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_DryRun_DoesNotPersistOutcome()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(t => t.WithStatus(SupportTaskStatus.Closed));
        await SetDataAsync(supportTask, supportTask.GetData<ChangeNameRequestData>() with { ChangeRequestOutcome = SupportRequestOutcome.Approved });

        // Act
        await ExecuteJobAsync(dryRun: true);

        // Assert
        Assert.Null(await GetOutcomeAsync(supportTask));
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTaskWithLegacyClosingEvent_BackfillsOutcomeOnEventPayload()
    {
        // Arrange
        var closedByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();
        var closingEventId = Guid.NewGuid();

        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);

            var data = supportTask.GetData<ChangeNameRequestData>() with { ChangeRequestOutcome = SupportRequestOutcome.Rejected };
            supportTask.Data = data;

            var newSupportTask = EventModels.SupportTask.FromModel(supportTask) with { Status = SupportTaskStatus.Closed, Outcome = null };
            var oldSupportTask = newSupportTask with { Status = SupportTaskStatus.Open };

            dbContext.AddEventWithoutBroadcast(new LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent
            {
                EventId = closingEventId,
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = closedByUser.UserId,
                PersonId = person.PersonId,
                RequestData = EventModels.ChangeNameRequestData.FromModel(data),
                RejectionReason = "Request and proof don't match",
                SupportTask = newSupportTask,
                OldSupportTask = oldSupportTask
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        await ExecuteJobAsync();

        // Assert
        Assert.Equal(SupportTaskOutcome.ChangeNameRequest_Rejected, await GetOutcomeAsync(supportTask));

        await WithDbContextAsync(async dbContext =>
        {
            var closingEvent = await dbContext.Events.SingleAsync(e => e.EventId == closingEventId);
            var eventBase = Assert.IsType<LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent>(closingEvent.ToEventBase());

            Assert.Equal(SupportTaskOutcome.ChangeNameRequest_Rejected, eventBase.SupportTask.Outcome);
            // The task wasn't closed yet at the point the event's old state was captured.
            Assert.Null(eventBase.OldSupportTask.Outcome);
        });
    }

    [Fact]
    public async Task ExecuteAsync_LegacyEventThatDoesNotCloseTask_IsLeftUnchanged()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var eventId = Guid.NewGuid();

        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            person.PersonId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);

            var data = supportTask.GetData<ChangeNameRequestData>() with { ChangeRequestOutcome = SupportRequestOutcome.Rejected };
            supportTask.Data = data;

            // Both states are open, so this event didn't close the task and carries no outcome.
            var newSupportTask = EventModels.SupportTask.FromModel(supportTask) with { Status = SupportTaskStatus.Open, Outcome = null };

            dbContext.AddEventWithoutBroadcast(new LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent
            {
                EventId = eventId,
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                RequestData = EventModels.ChangeNameRequestData.FromModel(data),
                RejectionReason = null,
                SupportTask = newSupportTask,
                OldSupportTask = newSupportTask
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        await ExecuteJobAsync();

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var @event = await dbContext.Events.SingleAsync(e => e.EventId == eventId);
            var eventBase = Assert.IsType<LegacyEvents.ChangeNameRequestSupportTaskRejectedEvent>(@event.ToEventBase());

            Assert.Null(eventBase.SupportTask.Outcome);
        });
    }

    private Task ExecuteJobAsync(bool dryRun = false) =>
        WithDbContextAsync(dbContext => new BackfillSupportTaskOutcomeJob(dbContext).ExecuteAsync(dryRun, CancellationToken.None));

    private Task<SupportTaskOutcome?> GetOutcomeAsync(SupportTask supportTask) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks
                .IgnoreQueryFilters()
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);

            return dbSupportTask.Outcome;
        });

    private Task SetDataAsync(SupportTask supportTask, ISupportTaskData data) =>
        WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);
            supportTask.Data = data;
            await dbContext.SaveChangesAsync();
        });

    private async Task<SupportTask> CreateClosedTrnRequestTaskAsync()
    {
        var result = await TestData.CreateTrnRequestSupportTaskAsync(
            configure: t => t.WithStatus(SupportTaskStatus.Closed));

        return result.SupportTask;
    }

    private async Task<SupportTask> CreateClosedTeacherPensionsTaskAsync()
    {
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        return await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            t =>
            {
                t.WithSupportTaskData("test.csv", 1);
                t.WithStatus(SupportTaskStatus.Closed);
            });
    }

    private async Task<SupportTask> CreateClosedNpqTrnRequestTaskAsync(NpqTrnRequestData data)
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var person = await TestData.CreatePersonAsync();

        var metadata = new TrnRequestMetadata
        {
            ApplicationUserId = applicationUser.UserId,
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
            Status = SupportTaskStatus.Closed,
            Data = data,
            PersonId = person.PersonId,
            TrnRequestApplicationUserId = applicationUser.UserId,
            TrnRequestId = metadata.RequestId,
            SubjectName = subject.Name,
            SubjectEmailAddress = subject.EmailAddress
        };

        return await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(metadata);
            dbContext.SupportTasks.Add(supportTask);
            await dbContext.SaveChangesAsync();
            return supportTask;
        });
    }

    private static TrnRequestDataPersonAttributes CreateTrnRequestAttributes() => new()
    {
        FirstName = "Alice",
        MiddleName = "",
        LastName = "Smith",
        DateOfBirth = new DateOnly(1990, 1, 1),
        EmailAddress = null,
        NationalInsuranceNumber = null,
        Gender = null
    };

    private static NpqTrnRequestDataPersonAttributes CreateNpqAttributes() => new()
    {
        FirstName = "Alice",
        MiddleName = "",
        LastName = "Smith",
        DateOfBirth = new DateOnly(1990, 1, 1),
        EmailAddress = null,
        NationalInsuranceNumber = null,
        Gender = null
    };

    private static TeacherPensionsPotentialDuplicateAttributes CreateTeacherPensionsAttributes() => new()
    {
        FirstName = "Alice",
        MiddleName = "",
        LastName = "Smith",
        DateOfBirth = new DateOnly(1990, 1, 1),
        NationalInsuranceNumber = null,
        Gender = null,
        Trn = "1234567"
    };
}

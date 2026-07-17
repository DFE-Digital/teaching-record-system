using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class BackfillResolvedAttributesJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task ExecuteAsync_MiddleNameTakenFromRequest_CorrectsResolvedMiddleNameToSelectedPerson()
    {
        // Arrange
        var existingMiddleName = "Existing";
        var requestMiddleName = "Different";

        var supportTask = await CreateResolvedTeacherPensionsTaskAsync(
            requestMiddleName,
            selected => selected with { MiddleName = existingMiddleName },
            // The bug: the request's middle name was recorded even though the person kept its own.
            resolved => resolved with { MiddleName = requestMiddleName });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillResolvedAttributesJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        var data = await GetTeacherPensionsDataAsync(supportTask);
        Assert.Equal(existingMiddleName, data.ResolvedAttributes!.MiddleName);
        Assert.Equal(existingMiddleName, data.SelectedPersonAttributes!.MiddleName);
    }

    [Fact]
    public async Task ExecuteAsync_AttributeDifferedFromRequest_LeavesCaseworkersChoiceUnchanged()
    {
        // Arrange
        // The merge page forces a choice whenever the values differ, so a differing resolved value is the
        // caseworker's selection and must survive.
        var requestFirstName = "Chosen";

        var supportTask = await CreateResolvedTeacherPensionsTaskAsync(
            requestMiddleName: null,
            selected => selected with { FirstName = "NotChosen" },
            resolved => resolved with { FirstName = requestFirstName },
            configureRequest: s => s.WithFirstName(requestFirstName));

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillResolvedAttributesJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        var data = await GetTeacherPensionsDataAsync(supportTask);
        Assert.Equal(requestFirstName, data.ResolvedAttributes!.FirstName);
    }

    [Fact]
    public async Task ExecuteAsync_TaskWithNoSelectedPersonAttributes_IsLeftUnchanged()
    {
        // Arrange
        // A task resolved without a merge records no attributes at all.
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithSupportTaskData("test.csv", 1);
                s.WithStatus(SupportTaskStatus.Closed);
            });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillResolvedAttributesJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        var data = await GetTeacherPensionsDataAsync(supportTask);
        Assert.Null(data.ResolvedAttributes);
        Assert.Null(data.SelectedPersonAttributes);
    }

    [Fact]
    public async Task ExecuteAsync_DryRun_DoesNotPersistCorrections()
    {
        // Arrange
        var requestMiddleName = "Different";

        var supportTask = await CreateResolvedTeacherPensionsTaskAsync(
            requestMiddleName,
            selected => selected with { MiddleName = "Existing" },
            resolved => resolved with { MiddleName = requestMiddleName });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillResolvedAttributesJob(dbContext).ExecuteAsync(dryRun: true, CancellationToken.None));

        // Assert
        var data = await GetTeacherPensionsDataAsync(supportTask);
        Assert.Equal(requestMiddleName, data.ResolvedAttributes!.MiddleName);
    }

    [Fact]
    public async Task ExecuteAsync_ClosedTaskWithLegacyClosingEvent_CorrectsResolvedAttributesOnEventPayload()
    {
        // Arrange
        var existingMiddleName = "Existing";
        var requestMiddleName = "Different";
        var closingEventId = Guid.NewGuid();

        var supportTask = await CreateResolvedTeacherPensionsTaskAsync(
            requestMiddleName,
            selected => selected with { MiddleName = existingMiddleName },
            resolved => resolved with { MiddleName = requestMiddleName });

        // The closing event embeds a copy of the task data, carrying the same wrong middle name.
        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks
                .Include(t => t.TrnRequestMetadata)
                .Include(t => t.Person)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);

            var newSupportTask = EventModels.SupportTask.FromModel(dbSupportTask);
            var oldSupportTask = newSupportTask with { Status = SupportTaskStatus.Open };

            dbContext.AddEventWithoutBroadcast(new LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent
            {
                EventId = closingEventId,
                CreatedUtc = TimeProvider.UtcNow,
                RaisedBy = SystemUser.SystemUserId,
                PersonId = dbSupportTask.PersonId!.Value,
                RequestData = EventModels.TrnRequestMetadata.FromModel(dbSupportTask.TrnRequestMetadata!),
                ChangeReason = LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedReason.RecordMerged,
                Changes = LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges.Status,
                PersonAttributes = EventModels.PersonDetails.FromModel(dbSupportTask.Person!),
                OldPersonAttributes = null,
                Comments = null,
                SupportTask = newSupportTask,
                OldSupportTask = oldSupportTask
            });

            await dbContext.SaveChangesAsync();
        });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillResolvedAttributesJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var closingEvent = await dbContext.Events.SingleAsync(e => e.EventId == closingEventId);
            var eventBase = Assert.IsType<LegacyEvents.TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent>(closingEvent.ToEventBase());
            var data = Assert.IsType<TeacherPensionsPotentialDuplicateData>(eventBase.SupportTask.Data);

            Assert.Equal(existingMiddleName, data.ResolvedAttributes!.MiddleName);
        });
    }

    // The NPQ journey's UI is gone, but the data it left behind has the same defect. It offered no name
    // choices, so a differing email address with no source selected is the equivalent case.
    [Fact]
    public async Task ExecuteAsync_NpqTaskWithEmailAddressTakenFromRequest_CorrectsResolvedEmailToSelectedPerson()
    {
        // Arrange
        // The request holds an empty email address and the record holds none, which the merge page treated as
        // not different, so no source was selected and the request's value was recorded regardless.
        var supportTask = await CreateResolvedNpqTaskAsync(
            requestEmailAddress: string.Empty,
            configureSelected: a => a with { EmailAddress = null },
            configureResolved: a => a with { EmailAddress = string.Empty });

        // Act
        await WithDbContextAsync(dbContext =>
            new BackfillResolvedAttributesJob(dbContext).ExecuteAsync(dryRun: false, CancellationToken.None));

        // Assert
        var data = await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            return dbSupportTask.GetData<NpqTrnRequestData>();
        });

        Assert.Null(data.ResolvedAttributes!.EmailAddress);
    }

    private async Task<SupportTask> CreateResolvedNpqTaskAsync(
        string? requestEmailAddress,
        Func<NpqTrnRequestDataPersonAttributes, NpqTrnRequestDataPersonAttributes> configureSelected,
        Func<NpqTrnRequestDataPersonAttributes, NpqTrnRequestDataPersonAttributes> configureResolved)
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
            EmailAddress = requestEmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            PotentialDuplicate = false
        };

        var baseline = new NpqTrnRequestDataPersonAttributes
        {
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            EmailAddress = person.EmailAddress,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender
        };

        var subject = SupportTask.Subject.FromTrnRequest(metadata);

        var supportTask = new SupportTask
        {
            CreatedOn = TimeProvider.UtcNow,
            UpdatedOn = TimeProvider.UtcNow,
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            Status = SupportTaskStatus.Closed,
            Data = new NpqTrnRequestData
            {
                SupportRequestOutcome = SupportRequestOutcome.Approved,
                SelectedPersonAttributes = configureSelected(baseline),
                ResolvedAttributes = configureResolved(baseline)
            },
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

    private async Task<SupportTask> CreateResolvedTeacherPensionsTaskAsync(
        string? requestMiddleName,
        Func<TeacherPensionsPotentialDuplicateAttributes, TeacherPensionsPotentialDuplicateAttributes> configureSelected,
        Func<TeacherPensionsPotentialDuplicateAttributes, TeacherPensionsPotentialDuplicateAttributes> configureResolved,
        Action<TestData.CreateTeacherPensionsPotentialDuplicateTaskBuilder>? configureRequest = null)
    {
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();

        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            s =>
            {
                s.WithMiddleName(requestMiddleName);
                s.WithSupportTaskData("test.csv", 1);
                s.WithStatus(SupportTaskStatus.Closed);
                configureRequest?.Invoke(s);
            });

        // The request's remaining attributes stand in for the merged-away record; the snapshot and resolved
        // attributes are what the resolve journey would have written.
        var baseline = new TeacherPensionsPotentialDuplicateAttributes
        {
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            DateOfBirth = person.DateOfBirth,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Gender = person.Gender,
            Trn = person.Trn!
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);
            supportTask.Data = supportTask.GetData<TeacherPensionsPotentialDuplicateData>() with
            {
                SelectedPersonAttributes = configureSelected(baseline),
                ResolvedAttributes = configureResolved(baseline)
            };
            await dbContext.SaveChangesAsync();
        });

        return supportTask;
    }

    private Task<TeacherPensionsPotentialDuplicateData> GetTeacherPensionsDataAsync(SupportTask supportTask) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            return dbSupportTask.GetData<TeacherPensionsPotentialDuplicateData>();
        });
}

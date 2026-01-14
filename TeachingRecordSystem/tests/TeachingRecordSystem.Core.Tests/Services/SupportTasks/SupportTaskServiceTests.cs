using System.Diagnostics;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks;

public class SupportTaskServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreateSupportTaskAsync_InvalidDataType_ThrowsInvalidOperationException()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());

        var options = new CreateSupportTaskOptions
        {
            SupportTaskType = SupportTaskType.ChangeNameRequest,
            Data = new ChangeDateOfBirthRequestData()
            {
                DateOfBirth = TestData.GenerateChangedDateOfBirth(person.DateOfBirth),
                EvidenceFileId = Guid.NewGuid(),
                EvidenceFileName = "evidence.jpeg",
                EmailAddress = person.EmailAddress!,
                ChangeRequestOutcome = null
            },
            PersonId = person.PersonId,
            OneLoginUserSubject = null,
            TrnRequest = null
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync<SupportTaskService, SupportTask>(
            service => service.CreateSupportTaskAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task CreateSupportTaskAsync_ValidRequest_CreatesSupportTaskAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithEmailAddress());

        var options = new CreateSupportTaskOptions
        {
            SupportTaskType = SupportTaskType.ChangeNameRequest,
            Data = new ChangeNameRequestData
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = TestData.GenerateChangedLastName(person.LastName),
                EvidenceFileId = Guid.NewGuid(),
                EvidenceFileName = "evidence.jpeg",
                EmailAddress = person.EmailAddress!,
                ChangeRequestOutcome = null
            },
            PersonId = person.PersonId,
            OneLoginUserSubject = null,
            TrnRequest = null
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, SupportTask>(
            service => service.CreateSupportTaskAsync(options, processContext));

        // Assert
        Assert.NotNull(result.SupportTaskReference);
        Assert.Equal(options.SupportTaskType, result.SupportTaskType);
        Assert.Equal(options.Data, result.Data);
        Assert.Equal(options.PersonId, result.PersonId);
        Assert.Equal(SupportTaskStatus.Open, result.Status);

        Events.AssertEventsPublished(e =>
        {
            var supportTaskCreatedEvent = Assert.IsType<SupportTaskCreatedEvent>(e);
            Assert.Equal(result.SupportTaskReference, supportTaskCreatedEvent.SupportTask.SupportTaskReference);
            Assert.Equal(result.SupportTaskType, supportTaskCreatedEvent.SupportTask.SupportTaskType);
            Assert.Equal(result.PersonId, supportTaskCreatedEvent.SupportTask.PersonId);
            Assert.Equal(result.Data, supportTaskCreatedEvent.SupportTask.Data);
            Assert.Equal(result.Status, supportTaskCreatedEvent.SupportTask.Status);
        });
    }

    [Fact]
    public async Task DeleteSupportTaskAsync_TaskDoesNotExist_ReturnsNotFoundAndDoesNotPublishEvent()
    {
        // Arrange
        var supportTaskReference = "ABC-123";

        var reasonDetail = Faker.Lorem.Paragraph();
        var options = new DeleteSupportTaskOptions(supportTaskReference, reasonDetail);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, DeleteSupportTaskResult>(
            service => service.DeleteSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(DeleteSupportTaskResult.NotFound, result);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task DeleteSupportTaskAsync_TaskIsAlreadyDeleted_ReturnsNotFoundAndDoesNotPublishEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);
            supportTask.DeletedOn = Clock.UtcNow.AddDays(-1);
            await dbContext.SaveChangesAsync();
        });

        var reasonDetail = Faker.Lorem.Paragraph();
        var options = new DeleteSupportTaskOptions(supportTask.SupportTaskReference, reasonDetail);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, DeleteSupportTaskResult>(
            service => service.DeleteSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(DeleteSupportTaskResult.NotFound, result);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task DeleteSupportTaskAsync_ValidRequest_DeletesTaskAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);

        var reasonDetail = Faker.Lorem.Paragraph();
        var options = new DeleteSupportTaskOptions(supportTask.SupportTaskReference, reasonDetail);

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, DeleteSupportTaskResult>(
            service => service.DeleteSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(DeleteSupportTaskResult.Ok, result);

        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.IgnoreQueryFilters().SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(processContext.Now, dbSupportTask.DeletedOn);
        });

        Events.AssertEventsPublished(e =>
        {
            var supportTaskDeletedEvent = Assert.IsType<SupportTaskDeletedEvent>(e);
            Assert.Equal(supportTask.SupportTaskReference, supportTaskDeletedEvent.SupportTaskReference);
            Assert.Equal(reasonDetail, supportTaskDeletedEvent.ReasonDetail);
        });
    }

    [Fact]
    public async Task UpdateSupportTaskAsync_TaskDoesNotExist_ReturnsNotFoundAndDoesNotPublishEvent()
    {
        // Arrange
        var supportTaskReference = "ABC-123";

        var options = new UpdateSupportTaskOptions<ChangeNameRequestData>
        {
            SupportTask = supportTaskReference,
            UpdateData = data => data,
            Status = SupportTaskStatus.Closed,
            Comments = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, UpdateSupportTaskResult>(
            service => service.UpdateSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(UpdateSupportTaskResult.NotFound, result);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task UpdateSupportTaskAsync_ValidRequestWithChangeOfData_UpdatesTaskAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var outcome = SupportRequestOutcome.Approved;

        var options = new UpdateSupportTaskOptions<ChangeNameRequestData>
        {
            SupportTask = supportTask.SupportTaskReference,
            UpdateData = data => data with { ChangeRequestOutcome = outcome },
            Status = SupportTaskStatus.Closed,
            Comments = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, UpdateSupportTaskResult>(
            service => service.UpdateSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(UpdateSupportTaskResult.Ok, result);

        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(options.Status, dbSupportTask.Status);
            Assert.Equal(outcome, ((ChangeNameRequestData)dbSupportTask.Data).ChangeRequestOutcome);
        });

        Events.AssertEventsPublished(e =>
        {
            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
            Assert.Equal(supportTask.SupportTaskReference, supportTaskUpdatedEvent.SupportTaskReference);
            Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
            Assert.Equal(options.Comments, supportTaskUpdatedEvent.Comments);
            Assert.Equal(SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data, supportTaskUpdatedEvent.Changes);
        });
    }

    [Fact]
    public async Task UpdateSupportTaskAsync_ValidRequestWithoutChangeOfData_UpdatesTaskAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var options = new UpdateSupportTaskOptions<ChangeNameRequestData>
        {
            SupportTask = supportTask.SupportTaskReference,
            UpdateData = data => data,
            Status = SupportTaskStatus.Closed,
            Comments = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, UpdateSupportTaskResult>(
            service => service.UpdateSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(UpdateSupportTaskResult.Ok, result);

        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(options.Status, dbSupportTask.Status);
        });

        Events.AssertEventsPublished(e =>
        {
            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
            Assert.Equal(supportTask.SupportTaskReference, supportTaskUpdatedEvent.SupportTaskReference);
            Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
            Assert.Equal(options.Comments, supportTaskUpdatedEvent.Comments);
            Assert.Equal(SupportTaskUpdatedEventChanges.Status, supportTaskUpdatedEvent.Changes);
        });
    }

    [Fact]
    public async Task UpdateSupportTaskAsync_ValidRequestWithNoChanges_DoesNotPublishEventOrSetUpdatedOn()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);
        Clock.Advance();

        var options = new UpdateSupportTaskOptions<ChangeNameRequestData>
        {
            SupportTask = supportTask.SupportTaskReference,
            UpdateData = data => data,
            Status = supportTask.Status,
            Comments = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, UpdateSupportTaskResult>(
            service => service.UpdateSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(UpdateSupportTaskResult.Ok, result);

        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(options.Status, dbSupportTask.Status);
            Assert.Equal(supportTask.CreatedOn, supportTask.UpdatedOn);
        });

        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task UpdateSupportTaskAsync_ValidRequestWithSavedJourneyState_UpdatesTaskAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId);
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var savedJourneyState = new SavedJourneyState(
            "Page",
            new Dictionary<string, string?>(),
            new DummyJourneyState(),
            typeof(DummyJourneyState));

        var options = new UpdateSupportTaskOptions<ChangeNameRequestData>
        {
            SupportTask = supportTask.SupportTaskReference,
            UpdateData = data => data,
            Status = SupportTaskStatus.InProgress,
            SavedJourneyState = Option.Some(savedJourneyState)!,
            Comments = Faker.Lorem.Paragraph()
        };

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<SupportTaskService, UpdateSupportTaskResult>(
            service => service.UpdateSupportTaskAsync(options, processContext));

        // Assert
        Assert.Equal(UpdateSupportTaskResult.Ok, result);

        await WithDbContextAsync(async dbContext =>
        {
            var dbSupportTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(options.Status, dbSupportTask.Status);
            Assert.Equal(savedJourneyState, dbSupportTask.ResolveJourneySavedState);
        });

        Events.AssertEventsPublished(e =>
        {
            var supportTaskUpdatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
            Assert.Equal(supportTask.SupportTaskReference, supportTaskUpdatedEvent.SupportTaskReference);
            Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
            Assert.Equal(options.Comments, supportTaskUpdatedEvent.Comments);
            Assert.Equal(SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.ResolveJourneySavedState, supportTaskUpdatedEvent.Changes);
        });
    }

    private record DummyJourneyState;
}

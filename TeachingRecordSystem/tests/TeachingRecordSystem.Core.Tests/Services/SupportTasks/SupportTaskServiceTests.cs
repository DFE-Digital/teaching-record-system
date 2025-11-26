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
}

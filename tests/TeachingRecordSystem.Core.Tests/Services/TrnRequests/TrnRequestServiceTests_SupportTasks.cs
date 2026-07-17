using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Tests.Services.TrnRequests;

public partial class TrnRequestServiceTests
{
    [Fact]
    public async Task CreateTrnRequestSupportTaskAsync_CreatesTaskAndPublishesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUser.UserId);

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(
            service => service.CreateTrnRequestSupportTaskAsync(
                new CreateTrnRequestSupportTaskOptions { TrnRequest = trnRequest },
                processContext));

        // Assert
        Assert.Equal(SupportTaskType.TrnRequest, result.SupportTaskType);
        Assert.IsType<TrnRequestData>(result.Data);
        Assert.Equal(SupportTaskStatus.Open, result.Status);
        Assert.Null(result.PersonId);
        Assert.Equal(trnRequest.ApplicationUserId, result.TrnRequestApplicationUserId);
        Assert.Equal(trnRequest.RequestId, result.TrnRequestId);

        Events.AssertEventsPublished(e =>
        {
            var createdEvent = Assert.IsType<SupportTaskCreatedEvent>(e);
            Assert.Equal(result.SupportTaskReference, createdEvent.SupportTask.SupportTaskReference);
            Assert.Equal(SupportTaskType.TrnRequest, createdEvent.SupportTask.SupportTaskType);
        });
    }

    [Fact]
    public async Task CreateManualChecksNeededSupportTaskAsync_CreatesTaskAndPublishesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUser.UserId);
        var person = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync(
            service => service.CreateManualChecksNeededSupportTaskAsync(
                new CreateManualChecksNeededSupportTaskOptions
                {
                    Person = person.Person,
                    TrnRequest = trnRequest
                },
                processContext));

        // Assert
        Assert.Equal(SupportTaskType.TrnRequestManualChecksNeeded, result.SupportTaskType);
        Assert.IsType<TrnRequestManualChecksNeededData>(result.Data);
        Assert.Equal(SupportTaskStatus.Open, result.Status);
        Assert.Equal(person.PersonId, result.PersonId);
        Assert.Equal(trnRequest.ApplicationUserId, result.TrnRequestApplicationUserId);
        Assert.Equal(trnRequest.RequestId, result.TrnRequestId);
        Assert.Equal(string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName), result.SubjectName);

        Events.AssertEventsPublished(e =>
        {
            var createdEvent = Assert.IsType<SupportTaskCreatedEvent>(e);
            Assert.Equal(result.SupportTaskReference, createdEvent.SupportTask.SupportTaskReference);
            Assert.Equal(SupportTaskType.TrnRequestManualChecksNeeded, createdEvent.SupportTask.SupportTaskType);
        });
    }

    [Fact]
    public async Task ResolveTrnRequestSupportTaskAsync_ClosesTaskWithAttributesAndPublishesEvent()
    {
        // Arrange
        var createResult = await TestData.CreateTrnRequestSupportTaskAsync();
        var supportTask = createResult.SupportTask;
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var resolvedAttributes = new TrnRequestDataPersonAttributes
        {
            FirstName = "Resolved",
            MiddleName = "Resolved Middle",
            LastName = "Resolved Last",
            DateOfBirth = new DateOnly(1990, 1, 1),
            EmailAddress = "resolved@example.com",
            NationalInsuranceNumber = "AB123456C",
            Gender = null
        };
        var selectedPersonAttributes = new TrnRequestDataPersonAttributes
        {
            FirstName = "Selected",
            MiddleName = "Selected Middle",
            LastName = "Selected Last",
            DateOfBirth = new DateOnly(1991, 2, 2),
            EmailAddress = "selected@example.com",
            NationalInsuranceNumber = "CD654321B",
            Gender = null
        };
        var comments = Faker.Lorem.Paragraph();

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(
            service => service.ResolveTrnRequestSupportTaskAsync(
                new ResolveTrnRequestSupportTaskOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference,
                    ResolvedAttributes = resolvedAttributes,
                    SelectedPersonAttributes = selectedPersonAttributes,
                    Comments = comments
                },
                processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, dbTask.Status);
            var data = Assert.IsType<TrnRequestData>(dbTask.Data);
            Assert.Equal(resolvedAttributes, data.ResolvedAttributes);
            Assert.Equal(selectedPersonAttributes, data.SelectedPersonAttributes);
        });

        Events.AssertEventsPublished(e =>
        {
            var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
            Assert.Equal(supportTask.SupportTaskReference, updatedEvent.SupportTaskReference);
            Assert.Equal(comments, updatedEvent.Comments);
            Assert.Equal(
                SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                updatedEvent.Changes);
        });
    }

    [Fact]
    public async Task ResolveTrnRequestSupportTaskAsync_TaskIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var createResult = await TestData.CreateTrnRequestSupportTaskAsync(
            configure: t => t.WithStatus(SupportTaskStatus.Closed));
        var supportTask = createResult.SupportTask;

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(
            service => service.ResolveTrnRequestSupportTaskAsync(
                new ResolveTrnRequestSupportTaskOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference,
                    ResolvedAttributes = null,
                    SelectedPersonAttributes = null
                },
                processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task CompleteManualChecksNeededSupportTaskAsync_ClosesTaskAndPublishesEvent()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync(
            service => service.CompleteManualChecksNeededSupportTaskAsync(
                new CompleteManualChecksNeededSupportTaskOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference
                },
                processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, dbTask.Status);
        });

        Events.AssertEventsPublished(e =>
        {
            var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
            Assert.Equal(supportTask.SupportTaskReference, updatedEvent.SupportTaskReference);
            Assert.Equal(SupportTaskUpdatedEventChanges.Status, updatedEvent.Changes);
        });
    }
}

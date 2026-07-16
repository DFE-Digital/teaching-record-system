using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

namespace TeachingRecordSystem.Core.Tests.Services.SupportTasks;

public class TeacherPensionsSupportTaskServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreatePotentialDuplicateAsync_CreatesTaskAndPublishesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await TestData.CreateDormantTrnRequestAsync(applicationUser.UserId);
        var person = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var result = await WithServiceAsync<TeacherPensionsSupportTaskService, SupportTask>(
            service => service.CreatePotentialDuplicateAsync(
                new CreateTeacherPensionsPotentialDuplicateOptions
                {
                    PersonId = person.PersonId,
                    TrnRequest = trnRequest,
                    FileName = "duplicates.csv",
                    IntegrationTransactionId = 42
                },
                processContext));

        // Assert
        Assert.Equal(SupportTaskType.TeacherPensionsPotentialDuplicate, result.SupportTaskType);
        Assert.Equal(SupportTaskStatus.Open, result.Status);
        Assert.Equal(person.PersonId, result.PersonId);
        Assert.Equal(trnRequest.ApplicationUserId, result.TrnRequestApplicationUserId);
        Assert.Equal(trnRequest.RequestId, result.TrnRequestId);

        var data = Assert.IsType<TeacherPensionsPotentialDuplicateData>(result.Data);
        Assert.Equal("duplicates.csv", data.FileName);
        Assert.Equal(42, data.IntegrationTransactionId);
        Assert.Null(data.ResolvedAttributes);
        Assert.Null(data.SelectedPersonAttributes);

        Events.AssertEventsPublished(e =>
        {
            var createdEvent = Assert.IsType<SupportTaskCreatedEvent>(e);
            Assert.Equal(result.SupportTaskReference, createdEvent.SupportTask.SupportTaskReference);
            Assert.Equal(SupportTaskType.TeacherPensionsPotentialDuplicate, createdEvent.SupportTask.SupportTaskType);
        });
    }

    [Fact]
    public async Task ResolveWithMergeAsync_ClosesTaskWithAttributesAndPublishesEvent()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            fileName: "duplicates.csv",
            integrationTransactionId: 42);
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var existingPerson = await TestData.CreatePersonAsync();

        var resolvedAttributes = new TeacherPensionsPotentialDuplicateAttributes
        {
            FirstName = "Resolved",
            MiddleName = "Resolved Middle",
            LastName = "Resolved Last",
            DateOfBirth = new DateOnly(1990, 1, 1),
            NationalInsuranceNumber = "AB123456C",
            Gender = null,
            Trn = "1000000"
        };
        var selectedPersonAttributes = new TeacherPensionsPotentialDuplicateAttributes
        {
            FirstName = "Selected",
            MiddleName = "Selected Middle",
            LastName = "Selected Last",
            DateOfBirth = new DateOnly(1991, 2, 2),
            NationalInsuranceNumber = "CD654321B",
            Gender = null,
            Trn = "2000000"
        };
        var comments = Faker.Lorem.Paragraph();

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync<TeacherPensionsSupportTaskService>(
            service => service.ResolveWithMergeAsync(
                new ResolveTeacherPensionsPotentialDuplicateWithMergeOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference,
                    ExistingPersonId = existingPerson.PersonId,
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
            var data = Assert.IsType<TeacherPensionsPotentialDuplicateData>(dbTask.Data);
            Assert.Equal(resolvedAttributes, data.ResolvedAttributes);
            Assert.Equal(selectedPersonAttributes, data.SelectedPersonAttributes);
            // The rest of the data is preserved by the update.
            Assert.Equal("duplicates.csv", data.FileName);
            Assert.Equal(42, data.IntegrationTransactionId);

            // The task's record is merged into the existing one, which the request resolves to
            var mergedPerson = await dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == supportTask.PersonId);
            Assert.Equal(PersonStatus.Deactivated, mergedPerson.Status);
            Assert.Equal(existingPerson.PersonId, mergedPerson.MergedWithPersonId);

            var trnRequest = await dbContext.TrnRequestMetadata.SingleAsync(r => r.RequestId == supportTask.TrnRequestId);
            Assert.Equal(existingPerson.PersonId, trnRequest.ResolvedPersonId);
        });

        Events.AssertEventsPublished(
            e =>
            {
                var deactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(e);
                Assert.Equal(supportTask.PersonId, deactivatedEvent.PersonId);
                Assert.Equal(existingPerson.PersonId, deactivatedEvent.MergedWithPersonId);
            },
            e =>
            {
                var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.Equal(supportTask.SupportTaskReference, updatedEvent.SupportTaskReference);
                Assert.Equal(comments, updatedEvent.Comments);
                Assert.Equal(
                    SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
                    updatedEvent.Changes);
            },
            e =>
            {
                var trnRequestUpdatedEvent = Assert.IsType<TrnRequestUpdatedEvent>(e);
                Assert.Equal(existingPerson.PersonId, trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);
            });
    }

    [Fact]
    public async Task ResolveWithMergeAsync_TaskIsAlreadyClosed_ThrowsInvalidOperationException()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateUserAsync();
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync(
            person.PersonId,
            user.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var existingPerson = await TestData.CreatePersonAsync();

        var ex = await Record.ExceptionAsync(() => WithServiceAsync<TeacherPensionsSupportTaskService>(
            service => service.ResolveWithMergeAsync(
                new ResolveTeacherPensionsPotentialDuplicateWithMergeOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference,
                    ExistingPersonId = existingPerson.PersonId,
                    ResolvedAttributes = null,
                    SelectedPersonAttributes = null
                },
                processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task ResolveWithoutMergeAsync_ClosesTaskAndPublishesEvent()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        Debug.Assert(supportTask.Status is SupportTaskStatus.Open);

        var comments = Faker.Lorem.Paragraph();

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync<TeacherPensionsSupportTaskService>(
            service => service.ResolveWithoutMergeAsync(
                new ResolveTeacherPensionsPotentialDuplicateWithoutMergeOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Comments = comments
                },
                processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, dbTask.Status);
            var data = Assert.IsType<TeacherPensionsPotentialDuplicateData>(dbTask.Data);
            Assert.Null(data.ResolvedAttributes);
            Assert.Null(data.SelectedPersonAttributes);

            // The task's record is kept, and the request resolves to it rather than being merged away
            var keptPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == supportTask.PersonId);
            Assert.Equal(PersonStatus.Active, keptPerson.Status);

            var trnRequest = await dbContext.TrnRequestMetadata.SingleAsync(r => r.RequestId == supportTask.TrnRequestId);
            Assert.Equal(supportTask.PersonId, trnRequest.ResolvedPersonId);
        });

        Events.AssertEventsPublished(
            e =>
            {
                var updatedEvent = Assert.IsType<SupportTaskUpdatedEvent>(e);
                Assert.Equal(supportTask.SupportTaskReference, updatedEvent.SupportTaskReference);
                Assert.Equal(comments, updatedEvent.Comments);
                Assert.Equal(SupportTaskUpdatedEventChanges.Status, updatedEvent.Changes);
            },
            e =>
            {
                var trnRequestUpdatedEvent = Assert.IsType<TrnRequestUpdatedEvent>(e);
                Assert.Equal(supportTask.PersonId, trnRequestUpdatedEvent.TrnRequest.ResolvedPersonId);
            });
    }
}

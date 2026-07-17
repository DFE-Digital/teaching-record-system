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

        var existingPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber());

        // Every attribute keeps the existing record's value, so the record itself doesn't change.
        var attributeSources = new PersonAttributeSources
        {
            FirstName = PersonAttributeSource.ExistingRecord,
            LastName = PersonAttributeSource.ExistingRecord,
            DateOfBirth = PersonAttributeSource.ExistingRecord,
            NationalInsuranceNumber = PersonAttributeSource.ExistingRecord,
            Gender = PersonAttributeSource.ExistingRecord
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
                    AttributeSources = attributeSources,
                    Comments = comments
                },
                processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, dbTask.Status);
            var data = Assert.IsType<TeacherPensionsPotentialDuplicateData>(dbTask.Data);

            var expectedAttributes = new TeacherPensionsPotentialDuplicateAttributes
            {
                FirstName = existingPerson.FirstName,
                MiddleName = existingPerson.MiddleName,
                LastName = existingPerson.LastName,
                DateOfBirth = existingPerson.DateOfBirth,
                NationalInsuranceNumber = existingPerson.NationalInsuranceNumber,
                Gender = existingPerson.Gender,
                Trn = existingPerson.Trn!
            };
            Assert.Equal(expectedAttributes, data.SelectedPersonAttributes);
            Assert.Equal(expectedAttributes, data.ResolvedAttributes);

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
    public async Task ResolveWithMergeAsync_AttributeSourcedFromRequest_UpdatesRecordAndSnapshotsItAsItWasBeforehand()
    {
        // Arrange
        var supportTask = await TestData.CreateTeacherPensionsPotentialDuplicateTaskAsync();
        var existingPerson = await TestData.CreatePersonAsync();

        var trnRequest = await WithDbContextAsync(dbContext =>
            dbContext.TrnRequestMetadata.SingleAsync(r => r.RequestId == supportTask.TrnRequestId));

        // Take the first name from the request; everything else keeps the existing record's value.
        var attributeSources = new PersonAttributeSources
        {
            FirstName = PersonAttributeSource.TrnRequest,
            LastName = PersonAttributeSource.ExistingRecord,
            DateOfBirth = PersonAttributeSource.ExistingRecord,
            NationalInsuranceNumber = PersonAttributeSource.ExistingRecord,
            Gender = PersonAttributeSource.ExistingRecord
        };

        var processContext = new ProcessContext(default, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync<TeacherPensionsSupportTaskService>(
            service => service.ResolveWithMergeAsync(
                new ResolveTeacherPensionsPotentialDuplicateWithMergeOptions
                {
                    SupportTaskReference = supportTask.SupportTaskReference,
                    ExistingPersonId = existingPerson.PersonId,
                    AttributeSources = attributeSources
                },
                processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPerson = await dbContext.Persons.SingleAsync(p => p.PersonId == existingPerson.PersonId);
            Assert.Equal(trnRequest.FirstName, updatedPerson.FirstName);
            Assert.Equal(existingPerson.LastName, updatedPerson.LastName);

            var dbTask = await dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            var data = Assert.IsType<TeacherPensionsPotentialDuplicateData>(dbTask.Data);

            // The snapshot is the record as it was before the merge, not after.
            Assert.Equal(existingPerson.FirstName, data.SelectedPersonAttributes!.FirstName);

            // The resolved attributes are the record as it is after the merge.
            Assert.Equal(trnRequest.FirstName, data.ResolvedAttributes!.FirstName);
            Assert.Equal(existingPerson.LastName, data.ResolvedAttributes.LastName);

            // The middle name has no choice on this journey, so the existing record's is kept.
            Assert.Equal(existingPerson.MiddleName, data.ResolvedAttributes.MiddleName);

            // The TRN is never resolvable; the surviving record keeps its own.
            Assert.Equal(existingPerson.Trn, data.ResolvedAttributes.Trn);
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
                    ExistingPersonId = existingPerson.PersonId
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

using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services;
using TeachingRecordSystem.Core.Services.MandatoryQualifications;

namespace TeachingRecordSystem.Core.Tests.Services.MandatoryQualifications;

public class MandatoryQualificationServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task CreateMandatoryQualificationAsync_AddsToDbAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var provider = MandatoryQualificationProvider.All.First();
        var startDate = new DateOnly(2021, 10, 5);
        var endDate = new DateOnly(2021, 11, 5);

        var options = new CreateMandatoryQualificationOptions
        {
            PersonId = person.PersonId,
            ProviderId = provider.MandatoryQualificationProviderId,
            Specialism = MandatoryQualificationSpecialism.Hearing,
            Status = MandatoryQualificationStatus.Passed,
            StartDate = startDate,
            EndDate = endDate
        };

        var processContext = new ProcessContext(ProcessType.MandatoryQualificationCreating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var mq = await WithServiceAsync<MandatoryQualificationService, MandatoryQualification>(
            service => service.CreateMandatoryQualificationAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbMq = await dbContext.MandatoryQualifications.FindAsync(mq.QualificationId);
            Assert.NotNull(dbMq);
            Assert.Equal(person.PersonId, dbMq.PersonId);
            Assert.Equal(provider.MandatoryQualificationProviderId, dbMq.ProviderId);
            Assert.Equal(MandatoryQualificationSpecialism.Hearing, dbMq.Specialism);
            Assert.Equal(MandatoryQualificationStatus.Passed, dbMq.Status);
            Assert.Equal(startDate, dbMq.StartDate);
            Assert.Equal(endDate, dbMq.EndDate);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<MandatoryQualificationCreatedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(mq.QualificationId, e.MandatoryQualification.QualificationId);
                Assert.Equal(provider.MandatoryQualificationProviderId, e.MandatoryQualification.Provider?.MandatoryQualificationProviderId);
                Assert.Equal(provider.Name, e.MandatoryQualification.Provider?.Name);
                Assert.Equal(MandatoryQualificationSpecialism.Hearing, e.MandatoryQualification.Specialism);
                Assert.Equal(MandatoryQualificationStatus.Passed, e.MandatoryQualification.Status);
            });
        });
    }

    [Fact]
    public async Task UpdateMandatoryQualificationAsync_WithChanges_UpdatesAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)
            .WithStatus(MandatoryQualificationStatus.Deferred)
            .WithStartDate(new DateOnly(2021, 10, 5))));
        var mq = person.MandatoryQualifications.Single();

        var options = new UpdateMandatoryQualificationOptions
        {
            QualificationId = mq.QualificationId,
            Specialism = Option.Some<MandatoryQualificationSpecialism?>(MandatoryQualificationSpecialism.Visual)
        };

        var processContext = new ProcessContext(ProcessType.MandatoryQualificationUpdating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var changes = await WithServiceAsync<MandatoryQualificationService, MandatoryQualificationUpdatedEventChanges>(
            service => service.UpdateMandatoryQualificationAsync(options, processContext));

        // Assert
        Assert.Equal(MandatoryQualificationUpdatedEventChanges.Specialism, changes);

        await WithDbContextAsync(async dbContext =>
        {
            var dbMq = await dbContext.MandatoryQualifications.FindAsync(mq.QualificationId);
            Assert.Equal(MandatoryQualificationSpecialism.Visual, dbMq!.Specialism);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<MandatoryQualificationUpdatedEvent>(e =>
            {
                Assert.Equal(MandatoryQualificationUpdatedEventChanges.Specialism, e.Changes);
                Assert.Equal(MandatoryQualificationSpecialism.Visual, e.MandatoryQualification.Specialism);
                Assert.Equal(MandatoryQualificationSpecialism.Hearing, e.OldMandatoryQualification.Specialism);
            });
        });
    }

    [Fact]
    public async Task UpdateMandatoryQualificationAsync_WithNoChanges_DoesNotPublishEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)
            .WithStatus(MandatoryQualificationStatus.Deferred)));
        var mq = person.MandatoryQualifications.Single();

        var options = new UpdateMandatoryQualificationOptions
        {
            QualificationId = mq.QualificationId,
            Specialism = Option.Some<MandatoryQualificationSpecialism?>(MandatoryQualificationSpecialism.Hearing)
        };

        var processContext = new ProcessContext(ProcessType.MandatoryQualificationUpdating, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        var changes = await WithServiceAsync<MandatoryQualificationService, MandatoryQualificationUpdatedEventChanges>(
            service => service.UpdateMandatoryQualificationAsync(options, processContext));

        // Assert
        Assert.Equal(MandatoryQualificationUpdatedEventChanges.None, changes);
        Events.AssertNoEventsPublished();
    }

    [Fact]
    public async Task DeleteMandatoryQualificationAsync_DeletesAndPublishesEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.MandatoryQualifications.Single();

        var options = new DeleteMandatoryQualificationOptions
        {
            QualificationId = mq.QualificationId
        };

        var processContext = new ProcessContext(ProcessType.MandatoryQualificationDeleting, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act
        await WithServiceAsync<MandatoryQualificationService>(
            service => service.DeleteMandatoryQualificationAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var dbMq = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleAsync(q => q.QualificationId == mq.QualificationId);
            Assert.NotNull(dbMq.DeletedOn);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.MandatoryQualificationDeleting, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<MandatoryQualificationDeletedEvent>(e =>
            {
                Assert.Equal(person.PersonId, e.PersonId);
                Assert.Equal(mq.QualificationId, e.MandatoryQualification.QualificationId);
            });
        });
    }

    [Fact]
    public async Task DeleteMandatoryQualificationAsync_AlreadyDeleted_Throws()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.MandatoryQualifications.Single();

        await WithDbContextAsync(async dbContext =>
        {
            var dbMq = await dbContext.MandatoryQualifications.SingleAsync(q => q.QualificationId == mq.QualificationId);
            dbMq.DeletedOn = TimeProvider.UtcNow;
            await dbContext.SaveChangesAsync();
        });

        var options = new DeleteMandatoryQualificationOptions
        {
            QualificationId = mq.QualificationId
        };

        var processContext = new ProcessContext(ProcessType.MandatoryQualificationDeleting, TimeProvider.UtcNow, SystemUser.SystemUserId);

        // Act & Assert
        // A soft-deleted MQ is excluded by the global query filter, so it can no longer be found.
        await Assert.ThrowsAsync<NotFoundException>(() =>
            WithServiceAsync<MandatoryQualificationService>(
                service => service.DeleteMandatoryQualificationAsync(options, processContext)));
    }
}

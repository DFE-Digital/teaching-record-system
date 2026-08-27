using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.Core.Tests.Jobs;

public class SetMissingHasEypsOnPersonsJobTests(JobFixture fixture) : JobTestBase(fixture)
{
    [Fact]
    public async Task Execute_PersonWithEypsRouteButHasEypsUnset_SetsHasEypsAndPublishesUpdatedEvent()
    {
        // Arrange
        var person = await CreatePersonWithUnsetHasEypsAsync();

        // Act
        await WithServiceAsync<SetMissingHasEypsOnPersonsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
            new Mock<IFileService>().Object);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.True(updated.HasEyps);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);

            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal("Data fix for incorrectly set Has EYPS flag", changeReason.Reason);

            p.AssertProcessHasEvents<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.Equal(person.PersonId, attributesEvent.PersonId);
                Assert.Equal(PersonProfessionalStatusAttributesUpdatedEventChanges.HasEyps, attributesEvent.Changes);
                Assert.True(attributesEvent.PersonAttributes.HasEyps);
                Assert.False(attributesEvent.OldPersonAttributes.HasEyps);
            });
        });
    }

    [Fact]
    public async Task Execute_MultiplePersonsToFix_PublishesOneProcessPerPerson()
    {
        // Arrange
        // Each person gets its own ProcessContext, so this covers publishing repeatedly within one job run.
        var person1 = await CreatePersonWithUnsetHasEypsAsync();
        var person2 = await CreatePersonWithUnsetHasEypsAsync();

        // Act
        await WithServiceAsync<SetMissingHasEypsOnPersonsJob>(
            job => job.ExecuteAsync(/*dryRun: */false, CancellationToken.None),
            new Mock<IFileService>().Object);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.Persons
                .Where(p => p.PersonId == person1.PersonId || p.PersonId == person2.PersonId)
                .ToListAsync();

            Assert.All(updated, p => Assert.True(p.HasEyps));
        });

        Events.AssertProcessesCreated(
            p => p.AssertProcessHasEvents<PersonProfessionalStatusAttributesUpdatedEvent>(),
            p => p.AssertProcessHasEvents<PersonProfessionalStatusAttributesUpdatedEvent>());
    }

    [Fact]
    public async Task Execute_DryRun_DoesNotCommitChanges()
    {
        // Arrange
        var person = await CreatePersonWithUnsetHasEypsAsync();

        // Act
        await WithServiceAsync<SetMissingHasEypsOnPersonsJob>(
            job => job.ExecuteAsync(/*dryRun: */true, CancellationToken.None),
            new Mock<IFileService>().Object);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updated = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.False(updated.HasEyps);

            var processes = await dbContext.Processes
                .Where(p => p.ProcessType == ProcessType.RouteToProfessionalStatusUpdating)
                .ToListAsync();
            Assert.Empty(processes);
        });
    }

    private async Task<Person> CreatePersonWithUnsetHasEypsAsync()
    {
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsProfessionalStatus));

        // The job looks for the flag being wrong on a person who does hold an EYPS route.
        await WithDbContextAsync(async dbContext =>
        {
            var toFix = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            toFix.HasEyps = false;
            await dbContext.SaveChangesAsync();
        });

        Events.Clear();

        return person;
    }
}

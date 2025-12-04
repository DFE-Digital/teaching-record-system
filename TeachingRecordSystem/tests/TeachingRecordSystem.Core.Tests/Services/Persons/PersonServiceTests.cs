using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Core.Tests.Services.Persons;

public class PersonServiceTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public async Task MergePersonAsync_PersonIsAlreadyDeactivated_ThrowsInvalidOperationException()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personToDeactivate.Person);
            personToDeactivate.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new MergePersonsOptions(personToDeactivate.PersonId, personToRetain.PersonId);

        // Act
        var ex = await Record.ExceptionAsync(() => WithServiceAsync(s => s.MergePersonsAsync(options, processContext)));

        // Assert
        Assert.IsType<InvalidOperationException>(ex);
    }

    [Fact]
    public async Task MergePersonAsync_ValidRequest_UpdatesPersonStatusAndPublishesEvent()
    {
        // Arrange
        var personToDeactivate = await TestData.CreatePersonAsync();
        var personToRetain = await TestData.CreatePersonAsync();

        var processContext = new ProcessContext(default, Clock.UtcNow, SystemUser.SystemUserId);

        var options = new MergePersonsOptions(personToDeactivate.PersonId, personToRetain.PersonId);

        // Act
        await WithServiceAsync(s => s.MergePersonsAsync(options, processContext));

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var deactivatedPerson = await dbContext.Persons.IgnoreQueryFilters().SingleAsync(p => p.PersonId == personToDeactivate.PersonId);
            Assert.Equal(PersonStatus.Deactivated, deactivatedPerson.Status);
            Assert.Equal(personToRetain.PersonId, deactivatedPerson.MergedWithPersonId);
        });

        Events.AssertEventsPublished(e =>
        {
            var personDeactivatedEvent = Assert.IsType<PersonDeactivatedEvent>(e);
            Assert.Equal(personToDeactivate.PersonId, personDeactivatedEvent.PersonId);
            Assert.Equal(personToRetain.PersonId, personDeactivatedEvent.MergedWithPersonId);
        });
    }

    private Task WithServiceAsync(Func<PersonService, Task> action, params object[] arguments) =>
        WithServiceAsync<PersonService>(action, arguments);

    private Task<TResult> WithServiceAsync<TResult>(Func<PersonService, Task<TResult>> action, params object[] arguments) =>
        WithServiceAsync<PersonService, TResult>(action, arguments);
}

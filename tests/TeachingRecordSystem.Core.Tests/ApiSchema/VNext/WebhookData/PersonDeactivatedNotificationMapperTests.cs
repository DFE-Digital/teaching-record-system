using TeachingRecordSystem.Core.ApiSchema.V3.V20260612.WebhookData;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.ApiSchema.VNext.WebhookData;

public class PersonDeactivatedNotificationMapperTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public Task MapEventAsync_WithMergedWithPerson_ReturnsExpectedNotification() =>
        WithServiceAsync<PersonDeactivatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = (await TestData.CreatePersonAsync()).Person;
            var anotherPerson = await TestData.CreatePersonAsync();

            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person);
                person.Status = PersonStatus.Deactivated;
                person.MergedWithPersonId = anotherPerson.PersonId;
                await dbContext.SaveChangesAsync();
            });

            var @event = new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                Changes = PersonDeactivatedEventChanges.PersonStatus | PersonDeactivatedEventChanges.MergedWithPersonId,
                PersonId = person.PersonId,
                MergedWithPersonId = anotherPerson.PersonId,
                DateOfDeath = null
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(person.Trn, notification.DeactivatedPerson.Trn);
            Assert.Equal(anotherPerson.Trn, notification.MergedWithPerson?.Trn);
        });

    [Fact]
    public Task MapEventAsync_WithoutMergedWithPerson_ReturnsExpectedNotification() =>
        WithServiceAsync<PersonDeactivatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = (await TestData.CreatePersonAsync()).Person;

            await WithDbContextAsync(async dbContext =>
            {
                dbContext.Attach(person);
                person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });

            var @event = new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                Changes = PersonDeactivatedEventChanges.PersonStatus,
                PersonId = person.PersonId,
                MergedWithPersonId = null,
                DateOfDeath = null
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(person.Trn, notification.DeactivatedPerson.Trn);
            Assert.Null(notification.MergedWithPerson);
        });
}

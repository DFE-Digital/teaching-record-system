using TeachingRecordSystem.Core.ApiSchema.V3.V20260224.WebhookData;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.ApiSchema.V20260224.WebhookData;

public class OneLoginUserUpdatedNotificationMapperTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public Task MapEventAsync_EmailAddressChanged_ReturnsNotification() =>
        WithServiceAsync<OneLoginUserUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                personId: person.PersonId,
                verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

            var oldEmailAddress = oneLoginUser.EmailAddress;
            var newEmailAddress = Faker.Internet.Email();

            await WithDbContextAsync(async dbContext =>
            {
                oneLoginUser.EmailAddress = newEmailAddress;
                await dbContext.SaveChangesAsync();
            });

            var @event = new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { EmailAddress = oldEmailAddress },
                Changes = OneLoginUserUpdatedEventChanges.EmailAddress
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(oneLoginUser.Subject, notification.OneLoginUser.Subject);
            Assert.Equal(newEmailAddress, notification.OneLoginUser.EmailAddress);
            Assert.NotNull(notification.ConnectedPerson);
            Assert.Equal(person.Trn, notification.ConnectedPerson.Trn);
        });

    [Fact]
    public Task MapEventAsync_PersonIdChangedToNonNull_ReturnsNotification() =>
        WithServiceAsync<OneLoginUserUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var oneLoginUser = await TestData.CreateOneLoginUserAsync();

            var @event = new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = person.PersonId },
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(oneLoginUser.Subject, notification.OneLoginUser.Subject);
            Assert.Equal(oneLoginUser.EmailAddress, notification.OneLoginUser.EmailAddress);
            Assert.NotNull(notification.ConnectedPerson);
            Assert.Equal(person.Trn, notification.ConnectedPerson.Trn);
        });

    [Fact]
    public Task MapEventAsync_PersonIdChangedToNull_ReturnsNotification() =>
        WithServiceAsync<OneLoginUserUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                personId: person.PersonId,
                verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

            var @event = new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = null },
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = person.PersonId },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(oneLoginUser.Subject, notification.OneLoginUser.Subject);
            Assert.Equal(oneLoginUser.EmailAddress, notification.OneLoginUser.EmailAddress);
            Assert.Null(notification.ConnectedPerson);
        });

    [Fact]
    public Task MapEventAsync_PersonIdChangedToDifferentPerson_ReturnsNotification() =>
        WithServiceAsync<OneLoginUserUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var oldPerson = await TestData.CreatePersonAsync();
            var newPerson = await TestData.CreatePersonAsync();
            var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                personId: oldPerson.PersonId,
                verifiedInfo: ([oldPerson.FirstName, oldPerson.LastName], oldPerson.DateOfBirth));

            var @event = new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = newPerson.PersonId },
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = oldPerson.PersonId },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(oneLoginUser.Subject, notification.OneLoginUser.Subject);
            Assert.Equal(oneLoginUser.EmailAddress, notification.OneLoginUser.EmailAddress);
            Assert.NotNull(notification.ConnectedPerson);
            Assert.Equal(newPerson.Trn, notification.ConnectedPerson.Trn);
        });

    [Fact]
    public Task MapEventAsync_NoRelevantChanges_ReturnsNull() =>
        WithServiceAsync<OneLoginUserUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                personId: person.PersonId,
                verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

            var @event = new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { VerifiedOn = Clock.UtcNow },
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser),
                Changes = OneLoginUserUpdatedEventChanges.VerifiedOn
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.Null(notification);
        });

    [Fact]
    public Task MapEventAsync_BothEmailAndPersonChanged_ReturnsNotification() =>
        WithServiceAsync<OneLoginUserUpdatedNotificationMapper>(async mapper =>
        {
            // Arrange
            var oldPerson = await TestData.CreatePersonAsync();
            var newPerson = await TestData.CreatePersonAsync();
            var oneLoginUser = await TestData.CreateOneLoginUserAsync(
                personId: oldPerson.PersonId,
                verifiedInfo: ([oldPerson.FirstName, oldPerson.LastName], oldPerson.DateOfBirth));

            var oldEmailAddress = oneLoginUser.EmailAddress;
            var newEmailAddress = Faker.Internet.Email();

            await WithDbContextAsync(async dbContext =>
            {
                oneLoginUser.EmailAddress = newEmailAddress;
                await dbContext.SaveChangesAsync();
            });

            var @event = new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { PersonId = newPerson.PersonId },
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(oneLoginUser) with { EmailAddress = oldEmailAddress, PersonId = oldPerson.PersonId },
                Changes = OneLoginUserUpdatedEventChanges.EmailAddress | OneLoginUserUpdatedEventChanges.PersonId
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(oneLoginUser.Subject, notification.OneLoginUser.Subject);
            Assert.Equal(newEmailAddress, notification.OneLoginUser.EmailAddress);
            Assert.NotNull(notification.ConnectedPerson);
            Assert.Equal(newPerson.Trn, notification.ConnectedPerson.Trn);
        });
}

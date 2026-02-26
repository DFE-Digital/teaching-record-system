using TeachingRecordSystem.Core.ApiSchema.V3.V20260224.WebhookData;
using TeachingRecordSystem.Core.Tests.Services;

namespace TeachingRecordSystem.Core.Tests.ApiSchema.V20260224.WebhookData;

public class TrnRequestCompletedNotificationMapperTests(ServiceFixture fixture) : ServiceTestBase(fixture)
{
    [Fact]
    public Task MapEventAsync_StatusChangedToCompleted_ReturnsNotification() =>
        WithServiceAsync<TrnRequestCompletedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

            var (supportTask, trnRequestMetadata, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
                applicationUserId: applicationUser.UserId,
                t => t
                    .WithFirstName(person.FirstName)
                    .WithMiddleName(person.MiddleName)
                    .WithLastName(person.LastName)
                    .WithDateOfBirth(person.DateOfBirth)
                    .WithEmailAddress(person.EmailAddress!)
                    .WithGender(person.Gender)
                    .WithMatches(false));

            await WithDbContextAsync(async dbContext =>
            {
                trnRequestMetadata.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);
                trnRequestMetadata.TrnToken = Guid.NewGuid().ToString();
                await dbContext.SaveChangesAsync();
            });

            var trnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata);

            var @event = new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = applicationUser.UserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = trnRequest,
                OldTrnRequest = trnRequest with { Status = TrnRequestStatus.Pending },
                ReasonDetails = null
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(trnRequest.RequestId, notification.TrnRequest.RequestId);
            Assert.Equal(person.Trn, notification.TrnRequest.Trn);
            Assert.Equal(TrnRequestStatus.Completed, notification.TrnRequest.Status);
            Assert.False(notification.TrnRequest.PotentialDuplicate);
            Assert.NotNull(notification.TrnRequest.AccessYourTeachingQualificationsLink);
        });

    [Fact]
    public Task MapEventAsync_WithPotentialDuplicate_ReturnsNotificationWithFlag() =>
        WithServiceAsync<TrnRequestCompletedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

            var (supportTask, trnRequestMetadata, matchedPersonIds) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
                applicationUserId: applicationUser.UserId,
                t => t
                    .WithFirstName(person.FirstName)
                    .WithMiddleName(person.MiddleName)
                    .WithLastName(person.LastName)
                    .WithDateOfBirth(person.DateOfBirth)
                    .WithEmailAddress(person.EmailAddress!)
                    .WithGender(person.Gender)
                    .WithMatchedPersons(person.PersonId));

            await WithDbContextAsync(async dbContext =>
            {
                trnRequestMetadata.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);
                trnRequestMetadata.TrnToken = Guid.NewGuid().ToString();
                await dbContext.SaveChangesAsync();
            });

            var trnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata);

            var @event = new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = applicationUser.UserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = trnRequest,
                OldTrnRequest = trnRequest with { Status = TrnRequestStatus.Pending },
                ReasonDetails = null
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.NotNull(notification);
            Assert.Equal(trnRequest.RequestId, notification.TrnRequest.RequestId);
            Assert.Equal(person.Trn, notification.TrnRequest.Trn);
            Assert.Equal(TrnRequestStatus.Completed, notification.TrnRequest.Status);
            Assert.True(notification.TrnRequest.PotentialDuplicate);
            Assert.NotNull(notification.TrnRequest.AccessYourTeachingQualificationsLink);
        });

    [Fact]
    public Task MapEventAsync_StatusChangedButNotToCompleted_ReturnsNull() =>
        WithServiceAsync<TrnRequestCompletedNotificationMapper>(async mapper =>
        {
            // Arrange
            var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

            var (supportTask, trnRequestMetadata, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
                applicationUserId: applicationUser.UserId);

            await WithDbContextAsync(async dbContext =>
            {
                trnRequestMetadata.SetRejected();
                await dbContext.SaveChangesAsync();
            });

            var trnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata);

            var @event = new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = applicationUser.UserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.Status,
                TrnRequest = trnRequest,
                OldTrnRequest = trnRequest with { Status = TrnRequestStatus.Pending },
                ReasonDetails = null
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.Null(notification);
        });

    [Fact]
    public Task MapEventAsync_StatusNotChanged_ReturnsNull() =>
        WithServiceAsync<TrnRequestCompletedNotificationMapper>(async mapper =>
        {
            // Arrange
            var person = await TestData.CreatePersonAsync();
            var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

            var (supportTask, trnRequestMetadata, _) = await TestData.CreateNpqTrnRequestSupportTaskAsync(
                applicationUserId: applicationUser.UserId,
                t => t
                    .WithFirstName(person.FirstName)
                    .WithMiddleName(person.MiddleName)
                    .WithLastName(person.LastName)
                    .WithDateOfBirth(person.DateOfBirth)
                    .WithEmailAddress(person.EmailAddress!)
                    .WithGender(person.Gender)
                    .WithMatches(false));

            await WithDbContextAsync(async dbContext =>
            {
                trnRequestMetadata.SetResolvedPerson(person.PersonId, TrnRequestStatus.Completed);
                trnRequestMetadata.TrnToken = Guid.NewGuid().ToString();
                await dbContext.SaveChangesAsync();
            });

            var trnRequest = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata);

            var @event = new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = applicationUser.UserId,
                RequestId = trnRequest.RequestId,
                Changes = TrnRequestUpdatedChanges.ResolvedPersonId,
                TrnRequest = trnRequest,
                OldTrnRequest = trnRequest with { ResolvedPersonId = null },
                ReasonDetails = null
            };

            // Act
            var notification = await mapper.MapEventAsync(@event);

            // Assert
            Assert.Null(notification);
        });
}

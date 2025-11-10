using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Core.Tests.Services.Webhooks;

public class WebhookDeliveryServiceTests(DbFixture dbFixture)
{
    public TestableClock Clock { get; } = new TestableClock();

    [Fact]
    public async Task SendMessagesAsync_SendsDueMessageAndUpdatesDb()
    {
        // Arrange
        var endpoint = await CreateApplicationUserAndEndpoint();
        var message = await CreateMessage(endpoint);

        var senderMock = new Mock<IWebhookSender>();

        var service = new WebhookDeliveryService(
            senderMock.Object,
            dbFixture.DbContextFactory,
            Clock,
            new NullLogger<WebhookDeliveryService>());

        // Act
        var result = await service.SendMessagesAsync();

        // Assert
        senderMock.Verify(mock => mock.SendMessageAsync(
            It.Is<WebhookMessage>(m => m.WebhookMessageId == message.WebhookMessageId),
            It.IsAny<CancellationToken>()));

        await dbFixture.WithDbContextAsync(async dbContext =>
        {
            await dbContext.Entry(message).ReloadAsync();
            Assert.Equal(Clock.UtcNow, message.Delivered);
            Assert.Collection(message.DeliveryAttempts, t => Assert.Equal(Clock.UtcNow, t));
        });
    }

    [Fact]
    public async Task SendMessagesAsync_DoesNotSendMessageDueInFuture()
    {
        // Arrange
        var endpoint = await CreateApplicationUserAndEndpoint();
        var message = await CreateMessage(endpoint, configureMessage: message => message.NextDeliveryAttempt = Clock.UtcNow.AddDays(1));

        var senderMock = new Mock<IWebhookSender>();

        var service = new WebhookDeliveryService(
            senderMock.Object,
            dbFixture.DbContextFactory,
            Clock,
            new NullLogger<WebhookDeliveryService>());

        // Act
        var result = await service.SendMessagesAsync();

        // Assert
        senderMock.Verify(mock => mock.SendMessageAsync(
            It.Is<WebhookMessage>(m => m.WebhookMessageId == message.WebhookMessageId),
            It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [Fact]
    public async Task SendMessagesAsync_NoOutstandingMessages_ReturnsMoreRecordsFalse()
    {
        // Arrange
        var endpoint = await CreateApplicationUserAndEndpoint();
        await CreateDueMessages(endpoint, WebhookDeliveryService.BatchSize);

        var senderMock = new Mock<IWebhookSender>();

        var service = new WebhookDeliveryService(
            senderMock.Object,
            dbFixture.DbContextFactory,
            Clock,
            new NullLogger<WebhookDeliveryService>());

        // Act
        var result = await service.SendMessagesAsync();

        // Assert
        Assert.False(result.MoreRecords);
    }

    [Fact]
    public async Task SendMessagesAsync_OutstandingMessages_ReturnsMoreRecordsTrue()
    {
        // Arrange
        var endpoint = await CreateApplicationUserAndEndpoint();
        await CreateDueMessages(endpoint, WebhookDeliveryService.BatchSize + 1);

        var senderMock = new Mock<IWebhookSender>();

        var service = new WebhookDeliveryService(
            senderMock.Object,
            dbFixture.DbContextFactory,
            Clock,
            new NullLogger<WebhookDeliveryService>());

        // Act
        var result = await service.SendMessagesAsync();

        // Assert
        Assert.True(result.MoreRecords);
    }

    [Fact]
    public async Task SendMessagesAsync_SenderFails_UpdatesDbWithDueTimeForRetry()
    {
        // Arrange
        var endpoint = await CreateApplicationUserAndEndpoint();
        var message = await CreateMessage(endpoint);

        var senderMock = new Mock<IWebhookSender>();

        var sendMessageExceptionMessage = "Bang!";
        senderMock
            .Setup(mock => mock.SendMessageAsync(
                It.Is<WebhookMessage>(m => m.WebhookMessageId == message.WebhookMessageId),
                It.IsAny<CancellationToken>()))
            .Throws(new Exception(sendMessageExceptionMessage))
            .Verifiable(Times.Once());

        var service = new WebhookDeliveryService(
            senderMock.Object,
            dbFixture.DbContextFactory,
            Clock,
            new NullLogger<WebhookDeliveryService>());

        // Act
        var result = await service.SendMessagesAsync();

        // Assert
        senderMock.Verify();

        await dbFixture.WithDbContextAsync(async dbContext =>
        {
            await dbContext.Entry(message).ReloadAsync();
            Assert.Null(message.Delivered);
            Assert.Collection(message.DeliveryAttempts, t => Assert.Equal(Clock.UtcNow, t));
            Assert.True(message.NextDeliveryAttempt > Clock.UtcNow);
            Assert.Collection(message.DeliveryErrors, e => Assert.Equal(sendMessageExceptionMessage, e));
        });
    }

    [Fact]
    public async Task SendMessagesAsync_SenderFailsAndNoMoreRetriesAllowed_UpdatesDbWithNullDueTime()
    {
        // Arrange
        var endpoint = await CreateApplicationUserAndEndpoint();

        var message = await CreateMessage(endpoint, timestamp: Clock.UtcNow.Subtract(TimeSpan.FromDays(365)), message =>
        {
            // Set up a message that's been attempted multiple times before and has failed every time but has a single retry left

            var attemptsAndErrors = WebhookDeliveryService.RetryIntervals.SkipLast(0).Prepend(TimeSpan.Zero).Aggregate(
                (Attempts: Array.Empty<DateTime>(), Errors: Array.Empty<string>()),
                (t, interval) => t with
                {
                    Attempts = [.. t.Attempts, t.Attempts.LastOrDefault(DateTime.SpecifyKind(message.Timestamp.DateTime, DateTimeKind.Utc)).Add(interval)],
                    Errors = [.. t.Errors, $"Error {t.Errors.Length + 1}"]
                });

            message.DeliveryAttempts = attemptsAndErrors.Attempts.ToList();
            message.DeliveryErrors = attemptsAndErrors.Errors.ToList();
        });

        var senderMock = new Mock<IWebhookSender>();

        var sendMessageExceptionMessage = "Bang!";
        senderMock
            .Setup(mock => mock.SendMessageAsync(
                It.Is<WebhookMessage>(m => m.WebhookMessageId == message.WebhookMessageId),
                It.IsAny<CancellationToken>()))
            .Throws(new Exception(sendMessageExceptionMessage))
            .Verifiable(Times.Once());

        var service = new WebhookDeliveryService(
            senderMock.Object,
            dbFixture.DbContextFactory,
            Clock,
            new NullLogger<WebhookDeliveryService>());

        // Act
        var result = await service.SendMessagesAsync();

        // Assert
        senderMock.Verify();

        await dbFixture.WithDbContextAsync(async dbContext =>
        {
            await dbContext.Entry(message).ReloadAsync();
            Assert.Null(message.Delivered);
            Assert.Equal(Clock.UtcNow, message.DeliveryAttempts.Last());
            Assert.Null(message.NextDeliveryAttempt);
            Assert.Equal(sendMessageExceptionMessage, message.DeliveryErrors.Last());
        });
    }

    private Task<WebhookEndpoint> CreateApplicationUserAndEndpoint() =>
        dbFixture.WithDbContextAsync(async dbContext =>
        {
            var applicationUser = new ApplicationUser()
            {
                UserId = Guid.NewGuid(),
                Name = $"Test user {Guid.NewGuid()}"
            };
            dbContext.ApplicationUsers.Add(applicationUser);

            var endpoint = new WebhookEndpoint()
            {
                Address = "http://localhost",
                ApiVersion = "20240101",
                ApplicationUserId = applicationUser.UserId,
                CloudEventTypes = [],
                Enabled = true,
                WebhookEndpointId = Guid.NewGuid(),
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };
            dbContext.WebhookEndpoints.Add(endpoint);

            await dbContext.SaveChangesAsync();

            return endpoint;
        });

    private Task CreateDueMessages(WebhookEndpoint endpoint, int count) =>
        dbFixture.WithDbContextAsync(async dbContext =>
        {
            for (var i = 0; i < count; i++)
            {
                var message = new WebhookMessage()
                {
                    ApiVersion = endpoint.ApiVersion,
                    CloudEventId = Guid.NewGuid().ToString(),
                    CloudEventType = "test.event",
                    Data = JsonSerializer.SerializeToElement(new
                    {
                        Foo = i
                    }),
                    Timestamp = Clock.UtcNow,
                    NextDeliveryAttempt = Clock.UtcNow,
                    WebhookEndpointId = endpoint.WebhookEndpointId,
                    WebhookMessageId = Guid.NewGuid()
                };
                dbContext.WebhookMessages.Add(message);
            }

            await dbContext.SaveChangesAsync();
        });

    private Task<WebhookMessage> CreateMessage(WebhookEndpoint endpoint, DateTime? timestamp = null, Action<WebhookMessage>? configureMessage = null) =>
        dbFixture.WithDbContextAsync(async dbContext =>
        {
            var message = new WebhookMessage()
            {
                ApiVersion = endpoint.ApiVersion,
                CloudEventId = Guid.NewGuid().ToString(),
                CloudEventType = "test.event",
                Data = JsonSerializer.SerializeToElement(new
                {
                    Foo = 42
                }),
                Timestamp = timestamp ?? Clock.UtcNow,
                NextDeliveryAttempt = Clock.UtcNow,
                WebhookEndpointId = endpoint.WebhookEndpointId,
                WebhookMessageId = Guid.NewGuid()
            };
            configureMessage?.Invoke(message);
            dbContext.WebhookMessages.Add(message);

            await dbContext.SaveChangesAsync();

            return message;
        });
}

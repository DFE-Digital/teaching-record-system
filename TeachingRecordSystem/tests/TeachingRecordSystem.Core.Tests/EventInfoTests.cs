using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.Core.Tests;

public class EventInfoTests
{
    [Fact]
    public void EventSerializesCorrectly()
    {
        // Arrange
        var @e = new UserActivatedEvent()
        {
            CreatedUtc = DateTime.UtcNow,
            SourceUserId = DataStore.Postgres.Models.User.SystemUserId,
            User = new()
            {
                AzureAdUserId = "ad-user-id",
                Email = "test.user@place.com",
                Name = "Test User",
                Roles = ["Administrator"],
                UserId = Guid.NewGuid(),
                UserType = UserType.Person
            }
        };

        var eventInfo = EventInfo.Create(@e);

        // Act
        var serialized = eventInfo.Serialize();
        var deserialized = EventInfo.Deserialize(serialized);

        // Assert
        var roundTripped = Assert.IsType<EventInfo<UserActivatedEvent>>(deserialized);
        Assert.Equal(e.CreatedUtc, roundTripped.Event.CreatedUtc);
        Assert.Equal(e.SourceUserId, roundTripped.Event.SourceUserId);
        Assert.Equal(e.User.AzureAdUserId, roundTripped.Event.User.AzureAdUserId);
        Assert.Equal(e.User.Email, roundTripped.Event.User.Email);
        Assert.Equal(e.User.Name, roundTripped.Event.User.Name);
        Assert.Equal(e.User.Roles, roundTripped.Event.User.Roles);
        Assert.Equal(e.User.UserId, roundTripped.Event.User.UserId);
        Assert.Equal(e.User.UserType, roundTripped.Event.User.UserType);
    }
}
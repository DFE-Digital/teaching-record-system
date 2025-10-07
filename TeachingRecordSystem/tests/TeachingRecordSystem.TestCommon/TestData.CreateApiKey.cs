using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<ApiKey> CreateApiKeyAsync(Guid applicationUserId, bool expired = false) => WithDbContextAsync(async dbContext =>
    {
        var expires = expired ? Clock.UtcNow.AddMinutes(-1) : (DateTime?)null;

        var apiKey = new ApiKey()
        {
            ApiKeyId = Guid.NewGuid(),
            ApplicationUserId = applicationUserId,
            CreatedOn = Clock.UtcNow,
            UpdatedOn = Clock.UtcNow,
            Key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            Expires = expires
        };

        dbContext.ApiKeys.Add(apiKey);

        var @event = new ApiKeyCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            ApiKey = EventModels.ApiKey.FromModel(apiKey)
        };
        dbContext.AddEventWithoutBroadcast(@event);

        await dbContext.SaveChangesAsync();

        return apiKey;
    });
}

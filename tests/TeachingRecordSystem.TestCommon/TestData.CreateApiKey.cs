using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<ApiKey> CreateApiKeyAsync(Guid applicationUserId, bool expired = false) => WithDbContextAsync(async dbContext =>
    {
        var expires = expired ? TimeProvider.UtcNow.AddMinutes(-1) : (DateTime?)null;

        var apiKey = new ApiKey()
        {
            ApiKeyId = Guid.NewGuid(),
            ApplicationUserId = applicationUserId,
            CreatedOn = TimeProvider.UtcNow,
            UpdatedOn = TimeProvider.UtcNow,
            Key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            Expires = expires
        };

        dbContext.ApiKeys.Add(apiKey);

        await dbContext.SaveChangesAsync();

        return apiKey;
    });
}

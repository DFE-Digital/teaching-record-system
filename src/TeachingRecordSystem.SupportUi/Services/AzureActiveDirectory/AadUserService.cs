using Microsoft.Graph;

namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public class AadUserService : IAadUserService
{
    private readonly GraphServiceClient _graphServiceClient;

    public AadUserService(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync(cancellationToken: cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new User()
        {
            UserId = user.Id!,
            Email = user.Mail!,
            Name = $"{user.GivenName} {user.Surname}"
        };
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await _graphServiceClient.Users.GetAsync(req =>
        {
            req.QueryParameters.Filter = $"mail eq '{email}'";
            req.Headers.Add("ConsistencyLevel", "eventual");
        }, cancellationToken);

        var user = result?.Value?.SingleOrDefault();

        if (user is null)
        {
            return null;
        }

        return new User()
        {
            UserId = user.Id!,
            Email = user.Mail!,
            Name = $"{user.GivenName} {user.Surname}"
        };
    }
}

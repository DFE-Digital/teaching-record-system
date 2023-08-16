using Microsoft.Graph;

namespace TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

public class UserService : IUserService
{
    private readonly GraphServiceClient _graphServiceClient;

    public UserService(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<User?> GetUserById(string userId)
    {
        var user = await _graphServiceClient.Users[userId].GetAsync();

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

    public async Task<User?> GetUserByEmail(string email)
    {
        var result = await _graphServiceClient.Users.GetAsync(req =>
        {
            req.QueryParameters.Filter = $"mail eq '{email}'";
            req.Headers.Add("ConsistencyLevel", "eventual");
        });

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

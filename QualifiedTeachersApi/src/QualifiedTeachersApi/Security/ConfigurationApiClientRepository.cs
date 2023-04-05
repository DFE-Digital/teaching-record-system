#nullable disable
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace QualifiedTeachersApi.Security;

public class ConfigurationApiClientRepository : IApiClientRepository
{
    private const string ConfigurationSection = "ApiClients";

    private readonly ApiClient[] _clients;

    public ConfigurationApiClientRepository(IConfiguration configuration)
    {
        _clients = GetClientsFromConfiguration(configuration);
    }

    public ApiClient GetClientByKey(string apiKey) => _clients.SingleOrDefault(c => c.ApiKey.Any(x => x == apiKey));

    private static ApiClient[] GetClientsFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(ConfigurationSection);
        return section.GetChildren().AsEnumerable()
            .Select((kvp, value) =>
            {
                var clientId = kvp.Key;
                var apiKey = kvp.GetSection("apiKey").Value;
                var client = new ApiClient()
                {
                    ClientId = clientId,
                    ApiKey = new List<string>()

                };
                kvp.Bind(client);
                if (!client.ApiKey.Any() && !string.IsNullOrEmpty(apiKey))
                    client.ApiKey.Add(apiKey);

                return client;
            })
            .ToArray();
    }
}

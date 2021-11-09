using System.Linq;
using Microsoft.Extensions.Configuration;

namespace DqtApi.Security
{
    public class ConfigurationApiClientRepository : IApiClientRepository
    {
        private const string ConfigurationSection = "ApiClients";

        private readonly ApiClient[] _clients;

        public ConfigurationApiClientRepository(IConfiguration configuration)
        {
            _clients = GetClientsFromConfiguration(configuration);
        }

        public ApiClient GetClientByKey(string apiKey) => _clients.SingleOrDefault(c => c.ApiKey == apiKey);

        private static ApiClient[] GetClientsFromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection(ConfigurationSection);

            return section.GetChildren().AsEnumerable()
                .Select(kvp =>
                {
                    var clientId = kvp.Key;

                    var client = new ApiClient()
                    {
                        ClientId = clientId
                    };
                    kvp.Bind(client);

                    return client;
                })
                .ToArray();
        }
    }
}

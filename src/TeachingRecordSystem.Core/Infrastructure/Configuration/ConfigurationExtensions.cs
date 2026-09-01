using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        public string GetRequiredValue(string key) =>
            configuration[key] ?? throw new Exception($"Missing '{key}' configuration entry.");

        public string GetRequiredConnectionString(string name) =>
            configuration.GetConnectionString(name) ?? throw new Exception($"Missing '{name}' connection string.");
    }
}

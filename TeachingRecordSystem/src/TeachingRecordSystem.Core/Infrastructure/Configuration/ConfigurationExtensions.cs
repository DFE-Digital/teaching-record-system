using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem;

public static class ConfigurationExtensions
{
    public static string GetRequiredValue(this IConfiguration configuration, string key) =>
        configuration[key] ?? throw new Exception($"Missing '{key}' configuration entry.");

    public static string GetRequiredConnectionString(this IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) ?? throw new Exception($"Missing '{name}' connection string.");
}

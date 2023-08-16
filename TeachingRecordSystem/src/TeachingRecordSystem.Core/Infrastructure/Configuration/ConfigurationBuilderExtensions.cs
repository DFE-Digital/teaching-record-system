using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem.Core.Infrastructure.Configuration;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonEnvironmentVariable(
        this IConfigurationBuilder builder,
        string environmentVariableName,
        string? configurationKeyPrefix = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (string.IsNullOrEmpty(environmentVariableName))
        {
            throw new ArgumentException("Environment variable name must be specified.", nameof(environmentVariableName));
        }

        return builder.Add(new EnvironmentVariableJsonConfigurationSource()
        {
            EnvironmentVariableName = environmentVariableName,
            ConfigurationKeyPrefix = configurationKeyPrefix
        });
    }
}

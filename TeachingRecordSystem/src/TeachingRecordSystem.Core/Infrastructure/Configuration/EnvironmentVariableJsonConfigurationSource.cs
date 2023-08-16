using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace TeachingRecordSystem.Core.Infrastructure.Configuration;

/// <summary>
/// Represents an environment variable with JSON contents as an <see cref="IConfigurationSource"/>.
/// </summary>
public class EnvironmentVariableJsonConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// The prefix to add to configuration keys.
    /// </summary>
    public string? ConfigurationKeyPrefix { get; set; }

    /// <summary>
    /// The environment variable to read JSON from.
    /// </summary>
    public required string EnvironmentVariableName { get; set; }

    /// <inheritdoc/>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        if (string.IsNullOrEmpty(EnvironmentVariableName))
        {
            throw new Exception($"{nameof(EnvironmentVariableName)} must be specified.");
        }

        return new WrapperSource(this).Build(builder);
    }

    private class WrapperSource : JsonStreamConfigurationSource
    {
        private const string EmptyJson = "{}";

        public WrapperSource(EnvironmentVariableJsonConfigurationSource source)
        {
            var envVar = Environment.GetEnvironmentVariable(source.EnvironmentVariableName) ?? EmptyJson;
            var envVarBytes = Encoding.ASCII.GetBytes(envVar);
            Stream = new MemoryStream(envVarBytes);

            ConfigurationKeyPrefix = source.ConfigurationKeyPrefix?.TrimEnd(':');  // ':' == ConfigurationPath.KeyDelimiter
        }

        public string? ConfigurationKeyPrefix { get; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder) => new WrapperProvider(this);
    }

    private class WrapperProvider : JsonStreamConfigurationProvider
    {
        private readonly WrapperSource _source;

        public WrapperProvider(WrapperSource source) : base(source)
        {
            _source = source;
        }

        public override void Load()
        {
            base.Load();

            if (!string.IsNullOrEmpty(_source.ConfigurationKeyPrefix))
            {
                foreach (var key in Data.Keys.ToArray())
                {
                    var prefixedKey = $"{_source.ConfigurationKeyPrefix}:{key}";
                    Data[prefixedKey] = Data[key];
                    Data.Remove(key);
                }
            }
        }
    }
}

namespace TeachingRecordSystem.SupportUi;

public interface IFeatureProvider
{
    bool IsEnabled(string featureName);
}

public class ConfigurationFeatureProvider(IConfiguration configuration) : IFeatureProvider
{
    private readonly HashSet<string> _features = new(configuration.GetSection("EnabledFeatures").Get<string[]>() ?? [], StringComparer.OrdinalIgnoreCase);

    public bool IsEnabled(string featureName) => _features.Contains(featureName);
}

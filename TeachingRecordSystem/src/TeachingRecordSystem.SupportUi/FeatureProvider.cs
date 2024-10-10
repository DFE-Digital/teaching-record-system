namespace TeachingRecordSystem.SupportUi;

public class FeatureProvider(IConfiguration configuration)
{
    private readonly HashSet<string> _features = new(configuration.GetSection("EnabledFeatures").Get<string[]>() ?? [], StringComparer.OrdinalIgnoreCase);

    public bool IsEnabled(string featureName) => _features.Contains(featureName);
}

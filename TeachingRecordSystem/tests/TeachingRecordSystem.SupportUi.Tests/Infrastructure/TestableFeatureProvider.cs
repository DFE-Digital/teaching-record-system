namespace TeachingRecordSystem.SupportUi.Tests.Infrastructure;

public class TestableFeatureProvider(IConfiguration configuration) : IFeatureProvider
{
    public ICollection<string> Features { get; } =
        new HashSet<string>(configuration.GetSection("EnabledFeatures").Get<string[]>() ?? [], StringComparer.OrdinalIgnoreCase);

    public bool IsEnabled(string featureName) => Features.Contains(featureName);
}

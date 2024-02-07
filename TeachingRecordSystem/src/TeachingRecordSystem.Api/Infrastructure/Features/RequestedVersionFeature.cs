namespace TeachingRecordSystem.Api.Infrastructure.Features;

public class RequestedVersionFeature(string? requestedMinorVersion)
{
    public string? RequestedMinorVersion { get; } = requestedMinorVersion;

    public string EffectiveMinorVersion => RequestedMinorVersion ?? VersionRegistry.DefaultV3MinorVersion;
}

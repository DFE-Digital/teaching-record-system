namespace TeachingRecordSystem.WebCommon.Infrastructure.Logging;

public class RedactedParametersMetadata
{
    public ICollection<string> ParameterNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}

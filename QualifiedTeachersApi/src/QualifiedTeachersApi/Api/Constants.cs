namespace QualifiedTeachersApi.Api;

public static class Constants
{
    public static IReadOnlyCollection<string> Versions { get; } = new[] { "v1", "v2", "v3" }.AsReadOnly();
}

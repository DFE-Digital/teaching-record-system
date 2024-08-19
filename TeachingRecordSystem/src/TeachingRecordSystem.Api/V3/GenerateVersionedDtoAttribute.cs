namespace TeachingRecordSystem.Api.V3;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GenerateVersionedDtoAttribute(Type sourceType, params string[] excludeMembers) : Attribute
{
    public Type SourceType { get; } = sourceType;

    public string[] ExcludeMembers { get; } = excludeMembers;
}

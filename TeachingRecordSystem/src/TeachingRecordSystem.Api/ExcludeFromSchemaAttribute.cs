namespace TeachingRecordSystem.Api;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ExcludeFromSchemaAttribute : Attribute
{
}

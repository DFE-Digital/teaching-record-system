namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ExcludeFromSchemaAttribute : Attribute
{
}

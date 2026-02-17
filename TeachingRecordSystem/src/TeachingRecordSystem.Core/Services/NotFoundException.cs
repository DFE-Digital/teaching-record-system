namespace TeachingRecordSystem.Core.Services;

public class NotFoundException(object id, string entityName) : Exception($"{entityName} with ID '{id}' was not found.")
{
    public object Id { get; } = id;
    public string EntityName { get; } = entityName;
}

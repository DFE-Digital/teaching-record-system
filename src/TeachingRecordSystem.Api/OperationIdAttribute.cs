namespace TeachingRecordSystem.Api;

// Sets the OpenAPI operationId. EndpointNameAttribute would also do this but endpoint names must be unique across the
// whole app, whereas each minor version redeclares the same operations.
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class OperationIdAttribute(string operationId) : Attribute
{
    public string OperationId { get; } = operationId;
}

namespace TeachingRecordSystem.Api.Webhooks;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CloudEventTypeAttribute(string type) : Attribute
{
    public string EventType { get; } = type;
}

namespace TeachingRecordSystem.FormFlow;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ActivatesJourneyAttribute : Attribute
{
}

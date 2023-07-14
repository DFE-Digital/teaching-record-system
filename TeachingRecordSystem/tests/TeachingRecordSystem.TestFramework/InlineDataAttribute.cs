namespace TeachingRecordSystem.TestFramework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class InlineDataAttribute : Attribute
{
    public InlineDataAttribute(params object?[] data)
    {
        // Handle [InlineData(null)]
        data ??= new object?[] { null };

        Data = data;
    }

    public object?[] Data { get; }
}

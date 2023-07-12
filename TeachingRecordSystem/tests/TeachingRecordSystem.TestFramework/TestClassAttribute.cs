namespace TeachingRecordSystem.TestFramework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class TestClassAttribute : Attribute
{
    public TestClassAttribute()
        : this(default)
    {
    }

    public TestClassAttribute(TestConcurrencyMode testConcurrencyMode)
    {
        TestConcurrencyMode = testConcurrencyMode;
    }

    public TestConcurrencyMode TestConcurrencyMode { get; }
}

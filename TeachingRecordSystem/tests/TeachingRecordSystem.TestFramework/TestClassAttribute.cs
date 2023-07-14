namespace TeachingRecordSystem.TestFramework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class TestClassAttribute : Attribute
{
    public TestClassAttribute()
    {
        TestConcurrencyMode = TestConcurrencyMode.Default;
    }

    public TestClassAttribute(TestConcurrencyMode testConcurrencyMode)
    {
        if (testConcurrencyMode == TestConcurrencyMode.Group)
        {
            throw new ArgumentException("Use the constructor that takes a group argument.");
        }

        TestConcurrencyMode = testConcurrencyMode;
    }

    public TestClassAttribute(string group)
    {
        TestConcurrencyMode = TestConcurrencyMode.Group;
        Group = group;
    }

    public TestConcurrencyMode TestConcurrencyMode { get; }

    public string? Group { get; }
}

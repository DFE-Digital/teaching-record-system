namespace TeachingRecordSystem.TestFramework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class TestSetupAttribute : Attribute
{
    public abstract Task Execute(TestInfo testInfo);
}

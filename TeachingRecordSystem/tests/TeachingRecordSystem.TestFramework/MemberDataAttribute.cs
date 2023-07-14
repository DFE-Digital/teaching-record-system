namespace TeachingRecordSystem.TestFramework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class MemberDataAttribute : Attribute
{
    public MemberDataAttribute(string memberName)
    {
        MemberName = memberName;
    }

    public string MemberName { get; }
}

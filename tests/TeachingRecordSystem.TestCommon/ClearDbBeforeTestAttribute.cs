namespace TeachingRecordSystem.TestCommon;

// Marks a test class whose data should be cleared down before each of its tests. InitializeDbFixture does the clearing;
// see the note there on why this isn't a BeforeAfterTestAttribute.
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class ClearDbBeforeTestAttribute : Attribute
{
    public virtual Task ClearAsync() => DbHelper.Instance.ClearDataAsync();
}

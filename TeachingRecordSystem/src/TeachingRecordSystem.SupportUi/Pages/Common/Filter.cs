namespace TeachingRecordSystem.SupportUi.Pages.Common;

public abstract class Filter<T>(string name, string displayName)
{
    public string Name => name;
    public string DisplayName => displayName;

    public abstract IQueryable<T> Apply(IQueryable<T> query);
}

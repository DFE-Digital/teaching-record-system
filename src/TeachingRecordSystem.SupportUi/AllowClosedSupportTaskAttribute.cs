using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TeachingRecordSystem.SupportUi;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowClosedSupportTaskAttribute : Attribute, IPageApplicationModelConvention
{
    public void Apply(PageApplicationModel model)
    {
        model.EndpointMetadata.Add(AllowClosedSupportTaskMetadata.Instance);
    }
}

public sealed class AllowClosedSupportTaskMetadata
{
    private AllowClosedSupportTaskMetadata() { }

    public static AllowClosedSupportTaskMetadata Instance { get; } = new();
}

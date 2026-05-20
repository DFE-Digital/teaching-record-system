using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace TeachingRecordSystem.SupportUi;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowDeactivatedPersonAttribute : Attribute, IPageApplicationModelConvention
{
    public void Apply(PageApplicationModel model)
    {
        model.EndpointMetadata.Add(AllowDeactivatedPersonMetadata.Instance);
    }
}

public sealed class AllowDeactivatedPersonMetadata
{
    private AllowDeactivatedPersonMetadata() { }

    public static AllowDeactivatedPersonMetadata Instance { get; } = new();
}

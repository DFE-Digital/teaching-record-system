using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.UiCommon.FormFlow;

public class FormFlowOptions
{
    public FormFlowOptions()
    {
        ValueProviderFactories = new List<IValueProviderFactory>()
        {
            new RouteValueProviderFactory(),
            new QueryStringValueProviderFactory()
        };

        JourneyRegistry = new();
    }

    public JourneyRegistry JourneyRegistry { get; }

    public MissingInstanceHandler MissingInstanceHandler { get; set; } = DefaultFormFlowOptions.MissingInstanceHandler;

    public IList<IValueProviderFactory> ValueProviderFactories { get; }
}

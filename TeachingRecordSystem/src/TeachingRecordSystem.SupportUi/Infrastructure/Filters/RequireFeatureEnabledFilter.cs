using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class RequireFeatureEnabledFilter(FeatureProvider featureProvider, string featureName) : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        if (!featureProvider.IsEnabled(featureName))
        {
            context.Result = new NotFoundResult();
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}

public class RequireFeatureEnabledFilterFactory(string featureName) : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -300;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<RequireFeatureEnabledFilter>(serviceProvider, featureName);
}

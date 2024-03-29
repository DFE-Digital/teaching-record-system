#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace TeachingRecordSystem.Api.Infrastructure.ModelBinding;

public static class MvcOptionsExtensions
{
    public static void AddHybridBodyModelBinderProvider(this MvcOptions options)
    {
        var bodyModelBinderProvider = options.ModelBinderProviders.OfType<BodyModelBinderProvider>().Single();
        var hybridBodyModelBinderProvider = new HybridBodyModelBinderProvider(bodyModelBinderProvider);
        options.ModelBinderProviders[options.ModelBinderProviders.IndexOf(bodyModelBinderProvider)] = hybridBodyModelBinderProvider;
    }
}

#nullable disable
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace QualifiedTeachersApi.Infrastructure.ModelBinding;

public static class MvcOptionsExtensions
{
    public static void AddHybridBodyModelBinderProvider(this MvcOptions options)
    {
        var bodyModelBinderProvider = options.ModelBinderProviders.OfType<BodyModelBinderProvider>().Single();
        var hybridBodyModelBinderProvider = new HybridBodyModelBinderProvider(bodyModelBinderProvider);
        options.ModelBinderProviders[options.ModelBinderProviders.IndexOf(bodyModelBinderProvider)] = hybridBodyModelBinderProvider;
    }
}

using System.Reflection;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.WebCommon.FormFlow;

internal class DiscoverJourneyDescriptors(Assembly assembly) : IConfigureOptions<FormFlowOptions>
{
    public void Configure(FormFlowOptions options)
    {
        var registerMethod = GetType().GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static)!;

        var registerJourneyTypes = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IRegisterJourney).IsAssignableFrom(t));

        foreach (var type in registerJourneyTypes)
        {
            registerMethod.MakeGenericMethod(type).Invoke(null, [options]);
        }
    }

#pragma warning disable IDE0051 // Remove unused private members
    private static void Register<T>(FormFlowOptions options) where T : IRegisterJourney
#pragma warning restore IDE0051 // Remove unused private members
    {
        options.JourneyRegistry.RegisterJourney(T.Journey);
    }
}

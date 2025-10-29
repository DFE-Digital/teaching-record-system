//using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Cli.Tests;
using TeachingRecordSystem.TestCommon;
//using Xunit.v3;

[assembly: AssemblyFixture(typeof(CompositionRoot))]

namespace TeachingRecordSystem.Cli.Tests;

public class CompositionRoot : IAsyncLifetime
{
    // static CompositionRoot()
    // {
    //     Xunit.v3.TypeActivator.Current = new TypeActivator(Instance.Services);
    // }

    public static CompositionRoot Instance => new();

    public IServiceProvider Services { get; } = CreateServiceProvider();

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = TestConfiguration.GetConfiguration();

        services
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton(DbHelper.Instance)
            .AddDatabase(configuration)
            .AddSingleton<TestData>()
            .AddSingleton<ReferenceDataCache>()
            .AddSingleton<IClock, TestableClock>()
            .AddSingleton<FakeTrnGenerator>();

        return services.BuildServiceProvider();
    }

    async ValueTask IAsyncLifetime.InitializeAsync() => await Services.GetRequiredService<DbHelper>().InitializeAsync();

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    // private class TypeActivator(IServiceProvider serviceProvider) : ITypeActivator
    // {
    //     public object CreateInstance(
    //         ConstructorInfo constructor,
    //         object?[]? arguments,
    //         Func<Type, IReadOnlyCollection<ParameterInfo>, string> missingArgumentMessageFormatter)
    //     {
    //         return ActivatorUtilities.CreateInstance(
    //             serviceProvider,
    //             constructor.DeclaringType!,
    //             (arguments ?? []).Where(a => a is not null));
    //     }
    // }
}

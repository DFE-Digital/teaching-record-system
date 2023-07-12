using System.Reflection;
using Fixie;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.TestFramework;

internal class Execution : IExecution
{
    private readonly TestEnvironment _environment;

    public Execution(TestEnvironment environment)
    {
        _environment = environment;
    }

    public async Task Run(TestSuite testSuite)
    {
        var startup = GetTestStartup();

        var configurationBuilder = new ConfigurationBuilder();
        startup.ConfigureConfiguration(configurationBuilder);
        var configuration = configurationBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfigurationRoot>(configuration);
        services.AddSingleton<IConfiguration>(sp => sp.GetRequiredService<IConfigurationRoot>());
        startup.ConfigureServices(services, configuration);
        await using var serviceProvider = services.BuildServiceProvider();

        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var concurrentTests = testSuite.TestClasses
            .Where(c => c.Type.GetCustomAttribute<TestClassAttribute>()!.TestConcurrencyMode == TestConcurrencyMode.Default)
            .SelectMany(tc => tc.Tests.Select(t => (Test: t, TestClass: tc)));

        await Parallel.ForEachAsync(concurrentTests, async (t, _) => await RunTest(t.Test, t.TestClass, serviceScopeFactory));

        var nonConcurrentTests = testSuite.TestClasses
            .Where(c => c.Type.GetCustomAttribute<TestClassAttribute>()!.TestConcurrencyMode == TestConcurrencyMode.NoConcurrency)
            .SelectMany(tc => tc.Tests.Select(t => (Test: t, TestClass: tc)));

        await Parallel.ForEachAsync(
            nonConcurrentTests,
            new ParallelOptions() { MaxDegreeOfParallelism = 1 },
            async (t, _) => await RunTest(t.Test, t.TestClass, serviceScopeFactory));
    }

    private ITestStartup GetTestStartup()
    {
        var startupInterfaceType = typeof(ITestStartup);

        var startupTypes = _environment.Assembly.GetTypes()
            .Where(t => t.IsPublic && !t.IsAbstract && startupInterfaceType.IsAssignableFrom(t))
            .ToArray();

        if (startupTypes.Length == 0)
        {
            throw new Exception("Cannot locate a test startup class.");
        }

        if (startupTypes.Length > 1)
        {
            throw new Exception("Multiple test startup classes found.");
        }

        var startupType = startupTypes.Single();
        return (ITestStartup)Activator.CreateInstance(startupType)!;
    }

    private async Task<object> CreateTestClassInstance(TestClass testClass, TestInfo testInfo, IServiceProvider testServices)
    {
        var testClassInstance = ActivatorUtilities.CreateInstance(testServices, testClass.Type);

        var setupAttributes = testClass.Type.GetCustomAttributes<TestSetupAttribute>(inherit: true);
        foreach (var setupAttribute in setupAttributes)
        {
            await setupAttribute.Execute(testInfo);
        }

        return testClassInstance;
    }

    private async Task RunTest(Test test, TestClass testClass, IServiceScopeFactory serviceScopeFactory)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();

        var testInfo = new TestInfo(scope.ServiceProvider, _environment.Console);
        TestInfo.SetCurrent(testInfo);

        var testClassInstance = await CreateTestClassInstance(testClass, testInfo, scope.ServiceProvider);

        try
        {
            await test.Run(testClassInstance);
        }
        finally
        {
            if (testClassInstance is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (testClassInstance is IDisposable disposable)
            {
                disposable.Dispose();
            }

            TestInfo.SetCurrent(null);
        }
    }
}

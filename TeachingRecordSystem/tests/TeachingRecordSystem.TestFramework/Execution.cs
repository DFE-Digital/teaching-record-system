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

        await startup.Initialize(serviceProvider);

        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var concurrentTests = testSuite.TestClasses
            .Select(c =>
            {
                var testClassAttribute = c.Type.GetCustomAttribute<TestClassAttribute>()!;
                return (testClassAttribute.TestConcurrencyMode, testClassAttribute.Group, TestClass: c);
            })
            .Where(t => t.TestConcurrencyMode == TestConcurrencyMode.Default || t.TestConcurrencyMode == TestConcurrencyMode.Group)
            .GroupBy(x => x.Group)
            .SelectMany(g =>
            {
                var group = g.Key;
                var groupLock = group is not null ? new SemaphoreSlim(1, 1) : null;
                return g.Select(t => (t.TestClass, GroupLock: groupLock));
            })
            .SelectMany(x => x.TestClass.Tests.Select(t => (Test: t, TestClass: x.TestClass, GroupLock: x.GroupLock)))
            .SelectMany(t => GetTestCases(t.Test, t.TestClass).Select(args => (Test: t.Test, TestClass: t.TestClass, Arguments: args, GroupLock: t.GroupLock)));

        await Parallel.ForEachAsync(concurrentTests, async (t, _) => await RunTest(t.Test, t.TestClass, t.Arguments, t.GroupLock, serviceScopeFactory));

        var nonConcurrentTests = testSuite.TestClasses
            .Where(c => c.Type.GetCustomAttribute<TestClassAttribute>()!.TestConcurrencyMode == TestConcurrencyMode.NoConcurrency)
            .SelectMany(tc => tc.Tests.Select(t => (Test: t, TestClass: tc)))
            .SelectMany(t => GetTestCases(t.Test, t.TestClass).Select(args => (Test: t.Test, TestClass: t.TestClass, Arguments: args)));

        await Parallel.ForEachAsync(
            nonConcurrentTests,
            new ParallelOptions() { MaxDegreeOfParallelism = 1 },
            async (t, _) => await RunTest(t.Test, t.TestClass, t.Arguments, groupLock: null, serviceScopeFactory));
    }

    private ITestStartup GetTestStartup()
    {
        var startupInterfaceType = typeof(ITestStartup);

        var startupTypes = _environment.Assembly.GetTypes()
            .Where(t => t.IsPublic && !t.IsAbstract && startupInterfaceType.IsAssignableFrom(t))
            .ToArray();

        if (startupTypes.Length == 0)
        {
            throw new TrsTestFrameworkException("Cannot locate a test startup class.");
        }

        if (startupTypes.Length > 1)
        {
            throw new TrsTestFrameworkException("Multiple test startup classes found.");
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

    private List<object?[]> GetTestCases(Test test, TestClass testClass)
    {
        List<object?[]> argumentGroups = new();

        if (test.HasParameters)
        {
            var memberDatas = test.Method.GetCustomAttributes<MemberDataAttribute>(inherit: false).ToArray();

            foreach (var memberData in memberDatas)
            {
                var member = testClass.Type.GetProperty(memberData.MemberName, BindingFlags.Public | BindingFlags.Static);

                if (member is null)
                {
                    throw new TrsTestFrameworkException($"Could not find member {memberData.MemberName} on {testClass.Type.Name}.");
                }

                var memberValue = member.GetValue(obj: null);

                if (memberValue is not TestArguments testArguments)
                {
                    throw new TrsTestFrameworkException("Member is not the correct type.");
                }

                foreach (var row in testArguments)
                {
                    if (row.Length != test.Parameters.Length)
                    {
                        throw new TrsTestFrameworkException("Incorrect number of arguments specified.");
                    }

                    argumentGroups.Add(row);
                }
            }

            var inlineDatas = test.Method.GetCustomAttributes<InlineDataAttribute>(inherit: false).ToArray();

            foreach (var inlineData in inlineDatas)
            {
                if (inlineData.Data.Length != test.Parameters.Length)
                {
                    throw new TrsTestFrameworkException("Incorrect number of arguments specified.");
                }

                argumentGroups.Add(inlineData.Data);
            }

            if (argumentGroups.Count == 0)
            {
                throw new TrsTestFrameworkException($"Could not find argument data.");
            }
        }
        else
        {
            argumentGroups.Add(Array.Empty<object?>());
        }

        return argumentGroups;
    }

    private async Task RunTest(Test test, TestClass testClass, object?[] arguments, SemaphoreSlim? groupLock, IServiceScopeFactory serviceScopeFactory)
    {
        if (groupLock is not null)
        {
            await groupLock.WaitAsync();
        }

        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();

            var testInfo = new TestInfo(scope.ServiceProvider, _environment.Console);
            TestInfo.SetCurrent(testInfo);

            var testClassInstance = await CreateTestClassInstance(testClass, testInfo, scope.ServiceProvider);

            try
            {
                await test.Run(testClassInstance, arguments);
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
        finally
        {
            groupLock?.Release();
        }
    }
}

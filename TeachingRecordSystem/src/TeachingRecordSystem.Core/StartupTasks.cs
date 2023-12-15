using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core;

public interface IStartupTask
{
    Task Execute();
}

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddStartupTask(this IServiceCollection services, IStartupTask task) =>
        AddStartupTask(services, _ => task);

    public static IServiceCollection AddStartupTask<T>(this IServiceCollection services) where T : class, IStartupTask =>
        AddStartupTask(services, sp => sp.GetService<T>() ?? ActivatorUtilities.CreateInstance<T>(sp));

    public static IServiceCollection AddStartupTask(this IServiceCollection services, Func<IServiceProvider, IStartupTask> createTask)
    {
        if (!services.Any(d => !d.IsKeyedService && d.ImplementationType == typeof(RunStartupTasksHostedService)))
        {
            services.Insert(0, ServiceDescriptor.Transient<IHostedService, RunStartupTasksHostedService>());
        }

        services.AddTransient<IStartupTask>(sp => createTask(sp));

        return services;
    }

    public static IServiceCollection AddStartupTask(this IServiceCollection services, Func<IServiceProvider, Task> action) =>
        AddStartupTask(services, sp => new DelegateStartupTask(sp, action));

    private class DelegateStartupTask : IStartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, Task> _action;

        public DelegateStartupTask(IServiceProvider serviceProvider, Func<IServiceProvider, Task> action)
        {
            _serviceProvider = serviceProvider;
            _action = action;
        }

        public Task Execute() => _action(_serviceProvider);
    }

    private class RunStartupTasksHostedService : IHostedService
    {
        private readonly IEnumerable<IStartupTask> _startupTasks;

        public RunStartupTasksHostedService(IEnumerable<IStartupTask> startupTasks)
        {
            _startupTasks = startupTasks;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var startupTask in _startupTasks)
            {
                await startupTask.Execute();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

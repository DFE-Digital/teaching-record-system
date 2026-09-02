using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core;

public interface IStartupTask
{
    Task ExecuteAsync();
}

public static partial class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddStartupTask(IStartupTask task) =>
            AddStartupTask(services, _ => task);

        public IServiceCollection AddStartupTask<T>() where T : class, IStartupTask =>
            AddStartupTask(services, sp => sp.GetService<T>() ?? ActivatorUtilities.CreateInstance<T>(sp));

        public IServiceCollection AddStartupTask(Func<IServiceProvider, IStartupTask> createTask)
        {
            if (!services.Any(d => !d.IsKeyedService && d.ImplementationType == typeof(RunStartupTasksHostedService)))
            {
                services.Insert(0, ServiceDescriptor.Transient<IHostedService, RunStartupTasksHostedService>());
            }

            services.AddTransient(sp => createTask(sp));

            return services;
        }

        public IServiceCollection AddStartupTask(Func<IServiceProvider, Task> action) =>
            AddStartupTask(services, sp => new DelegateStartupTask(sp, action));
    }

    private class DelegateStartupTask : IStartupTask
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, Task> _action;

        public DelegateStartupTask(IServiceProvider serviceProvider, Func<IServiceProvider, Task> action)
        {
            _serviceProvider = serviceProvider;
            _action = action;
        }

        public Task ExecuteAsync() => _action(_serviceProvider);
    }

    private class RunStartupTasksHostedService : IHostedLifecycleService
    {
        private readonly IEnumerable<IStartupTask> _startupTasks;

        public RunStartupTasksHostedService(IEnumerable<IStartupTask> startupTasks)
        {
            _startupTasks = startupTasks;
        }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StartingAsync(CancellationToken cancellationToken)
        {
            foreach (var startupTask in _startupTasks)
            {
                await startupTask.ExecuteAsync();
            }
        }

        public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

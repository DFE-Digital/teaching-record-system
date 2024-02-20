using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Processing;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class PublishEventsDbCommandInterceptor : SaveChangesInterceptor
{
    private readonly IEventObserver _eventObserver;

    public PublishEventsDbCommandInterceptor(IEventObserver eventObserver)
    {
        _eventObserver = eventObserver;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var events = eventData.Context!.ChangeTracker.Entries<Event>();

        foreach (var e in events)
        {
            if (e.State == EntityState.Added)
            {
                e.Property(e => e.Published).CurrentValue = true;

                eventData.Context.SavedChanges += OnEventSaved;

                void OnEventSaved(object? sender, SavedChangesEventArgs args)
                {
                    _eventObserver.OnEventSaved(e.Entity.ToEventBase()).GetAwaiter().GetResult();
                    eventData.Context.SavedChanges -= OnEventSaved;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PublishEventsDbCommandInterceptor>();

        services.Decorate<DbContextOptions<TrsDbContext>>((inner, sp) =>
        {
            var coreOptionsExtension = inner.GetExtension<CoreOptionsExtension>();

            return (DbContextOptions<TrsDbContext>)inner.WithExtension(
                coreOptionsExtension.WithInterceptors(new IInterceptor[]
                {
                        sp.GetRequiredService<PublishEventsDbCommandInterceptor>(),
                }));
        });

        return services;
    }
}

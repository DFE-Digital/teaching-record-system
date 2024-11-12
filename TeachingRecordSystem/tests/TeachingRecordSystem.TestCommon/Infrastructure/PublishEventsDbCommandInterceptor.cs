using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.TestCommon.Infrastructure;

public class PublishEventsDbCommandInterceptor(IEventObserver eventObserver) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        PublishEvents(eventData);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        PublishEvents(eventData);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PublishEventsDbCommandInterceptor>();

        services.Decorate<DbContextOptions<TrsDbContext>>((inner, sp) =>
        {
            var coreOptionsExtension = inner.GetExtension<CoreOptionsExtension>();

            return (DbContextOptions<TrsDbContext>)inner.WithExtension(
                coreOptionsExtension.WithInterceptors(
                [
                    sp.GetRequiredService<PublishEventsDbCommandInterceptor>(),
                ]));
        });

        return services;
    }

    private void PublishEvents(DbContextEventData eventData)
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
                    eventObserver.OnEventCreated(e.Entity.ToEventBase());
                    eventData.Context.SavedChanges -= OnEventSaved;
                }
            }
        }
    }
}

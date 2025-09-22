using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Events.EventHandlers;

namespace TeachingRecordSystem.Core.Events;

public class EventPublisher(IServiceProvider serviceProvider, TrsDbContext dbContext) : IEventPublisher
{
    public async Task PublishEventAsync(EventBase @event)
    {
        _ = Transaction.Current ?? throw new InvalidOperationException("A TransactionScope is required to publish an event.");

        if (dbContext.ChangeTracker.HasChanges())
        {
            throw new InvalidOperationException("DbContext has pending changes; call SaveChanges() before publishing an event.");
        }

        var eventType = @event.GetType();

        var nonGenericHandlers = serviceProvider.GetServices<IEventHandler>();
        foreach (var handler in nonGenericHandlers)
        {
            await handler.HandleAsync(@event);
        }

        var genericHandlers = serviceProvider.GetServices(typeof(IEventHandler<>).MakeGenericType(eventType));
        foreach (var handler in genericHandlers)
        {
            var wrapperType = typeof(HandlerWrapper<>).MakeGenericType(eventType);
            var wrapper = (HandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;
            await wrapper.HandleAsync(@event);
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task HandleAsync(EventBase @event);
    }

    private class HandlerWrapper<TEvent>(IEventHandler<TEvent> handler) : HandlerWrapper where TEvent : EventBase
    {
        public override Task HandleAsync(EventBase @event)
        {
            return handler.HandleAsync((TEvent)@event);
        }
    }
}

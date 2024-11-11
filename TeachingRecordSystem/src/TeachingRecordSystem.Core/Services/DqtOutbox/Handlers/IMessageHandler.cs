namespace TeachingRecordSystem.Core.Services.DqtOutbox.Handlers;

public interface IMessageHandler<TMessage>
{
    Task HandleMessage(TMessage message);
}

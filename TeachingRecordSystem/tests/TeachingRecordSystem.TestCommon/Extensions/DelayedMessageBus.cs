using Xunit.Sdk;
using Xunit.v3;

namespace TeachingRecordSystem.TestCommon.Extensions;

/// <summary>
/// Used to capture messages to potentially be forwarded later. Messages are forwarded by
/// disposing of the message bus.
/// </summary>
public sealed class DelayedMessageBus(IMessageBus innerBus) : IMessageBus
{
    private readonly List<IMessageSinkMessage> _messages = [];

    public bool QueueMessage(IMessageSinkMessage message)
    {
        // Technically speaking, this lock isn't necessary in our case, because we know we're using this
        // message bus for a single test (so there's no possibility of parallelism). However, it's good
        // practice when something might be used where parallel messages might arrive, so it's here in
        // this sample.
        lock (_messages)
        {
            _messages.Add(message);
        }

        // No way to ask the inner bus if they want to cancel without sending them the message, so
        // we just go ahead and continue always.
        return true;
    }

    public void Dispose()
    {
        foreach (var message in _messages)
        {
            innerBus.QueueMessage(message);
        }
    }
}

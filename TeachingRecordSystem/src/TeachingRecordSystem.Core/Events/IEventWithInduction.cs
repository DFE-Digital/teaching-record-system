using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithInduction
{
    Induction Induction { get; }
    Induction OldInduction { get; }
}

using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithInduction
{
    Induction Induction { get; }
    Induction OldInduction { get; }
}

namespace TeachingRecordSystem.Core.Events;

public interface IEventWithSecondaryPersonId
{
    Guid SecondaryPersonId { get; }
}

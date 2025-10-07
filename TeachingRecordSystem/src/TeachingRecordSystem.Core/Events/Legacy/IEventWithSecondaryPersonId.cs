namespace TeachingRecordSystem.Core.Events.Legacy;

public interface IEventWithSecondaryPersonId
{
    Guid SecondaryPersonId { get; }
}

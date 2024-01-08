using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

public record TimelineEvent(EventBase Event, RaisedByUser RaisedByUser);

public record TimelineEvent<TEvent>(TEvent Event, RaisedByUser RaisedByUser) : TimelineEvent(Event, RaisedByUser) where TEvent : EventBase
{
    public new TEvent Event => (TEvent)base.Event;
}

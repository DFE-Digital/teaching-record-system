namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

public record TimelineEvent(EventBase Event, RaisedByUserInfo RaisedByUser);

public record TimelineEvent<TEvent>(TEvent Event, RaisedByUserInfo RaisedByUser) : TimelineEvent(Event, RaisedByUser) where TEvent : EventBase
{
    public new TEvent Event => (TEvent)base.Event;
}

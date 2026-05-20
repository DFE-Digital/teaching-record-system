using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.Timeline.Events;

public record TimelineEvent(EventBase Event, RaisedByUserInfo RaisedByUser, ApplicationUserInfo? ApplicationUser);

public record TimelineEvent<TEvent>(TEvent Event, RaisedByUserInfo RaisedByUser, ApplicationUserInfo? ApplicationUser) : TimelineEvent(Event, RaisedByUser, ApplicationUser) where TEvent : EventBase
{
    public new TEvent Event => (TEvent)base.Event;
}

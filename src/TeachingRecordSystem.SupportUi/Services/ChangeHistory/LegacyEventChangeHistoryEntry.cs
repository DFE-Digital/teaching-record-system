using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public record LegacyEventChangeHistoryEntry(EventBase Event, RaisedByUserInfo RaisedByUser, ApplicationUserInfo? ApplicationUser);

public record LegacyEventChangeHistoryEntry<TEvent>(TEvent Event, RaisedByUserInfo RaisedByUser, ApplicationUserInfo? ApplicationUser) : LegacyEventChangeHistoryEntry(Event, RaisedByUser, ApplicationUser) where TEvent : EventBase
{
    public new TEvent Event => (TEvent)base.Event;
}

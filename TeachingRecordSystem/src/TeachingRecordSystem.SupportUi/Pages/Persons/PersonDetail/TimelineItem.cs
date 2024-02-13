namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public record TimelineItem(TimelineItemType ItemType, DateTime Timestamp, object ItemModel)
{
    public const string TimestampFormat = "d MMMMM yyyy 'at' h:mm tt";

    public string FormattedTimestamp => Timestamp.ToString(TimestampFormat);
}

public record TimelineItem<TModel>(TimelineItemType ItemType, DateTime Timestamp, TModel ItemModel) : TimelineItem(ItemType, Timestamp, ItemModel) where TModel : notnull
{
    public new TModel ItemModel => (TModel)base.ItemModel;
}

public enum TimelineItemType
{
    Annotation,
    IncidentResolution,
    Task,
    Event
}

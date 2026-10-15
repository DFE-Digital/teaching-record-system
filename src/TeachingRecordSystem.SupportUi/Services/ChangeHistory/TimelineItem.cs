namespace TeachingRecordSystem.SupportUi.Services.ChangeHistory;

public record TimelineItem(Guid PersonId, DateTime Timestamp, object ItemModel)
{
    public const string TimestampFormat = "d MMMMM yyyy 'at' h:mm tt";

    public string FormattedGmtTimestamp => Timestamp.ToGmt().ToString(TimestampFormat);
}

public record TimelineItem<TModel>(Guid PersonId, DateTime Timestamp, TModel ItemModel) : TimelineItem(PersonId, Timestamp, ItemModel) where TModel : notnull
{
    public new TModel ItemModel => (TModel)base.ItemModel;
}

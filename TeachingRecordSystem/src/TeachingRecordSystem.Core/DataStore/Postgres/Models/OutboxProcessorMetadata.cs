namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class OutboxMessageProcessorMetadata
{
    public required string Key { get; init; }
    public required string Value { get; set; }

    public static class Keys
    {
        public const string IgnoreUntil = "IgnoreUntil";
    }
}

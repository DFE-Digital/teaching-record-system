namespace Dfe.Analytics.EFCore.AirbyteApi;

public record UpdateConnectionDetailsRequest
{
    public required IEnumerable<UpdateConnectionDetailsRequestConfiguration> Configurations { get; set; }
}

public record UpdateConnectionDetailsRequestConfiguration
{
    public required IEnumerable<UpdateConnectionDetailsRequestConfigurationStream> Streams { get; set; }
}

public record UpdateConnectionDetailsRequestConfigurationStream
{
    public required string Name { get; set; }
    public required string SyncMode { get; set; }
    public required IEnumerable<string> CursorField { get; set; }
    public required IEnumerable<IEnumerable<string>> PrimaryKey { get; set; }
    public required IEnumerable<UpdateConnectionDetailsRequestConfigurationStreamField> SelectedFields { get; set; }
}

public record UpdateConnectionDetailsRequestConfigurationStreamField
{
    public required IEnumerable<string> FieldPath { get; set; }
}

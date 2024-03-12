namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TpsCsvExtract
{
    public const int FilenameMaxLength = 200;

    public required Guid TpsCsvExtractId { get; set; }
    public required string Filename { get; set; }
    public required DateTime CreatedOn { get; set; }
}

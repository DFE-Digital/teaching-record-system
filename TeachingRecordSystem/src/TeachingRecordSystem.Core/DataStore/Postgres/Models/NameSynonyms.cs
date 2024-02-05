namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class NameSynonyms
{
    public const int NameMaxLength = 100;
    public const string NameSynonymsIndexName = "ix_name_synonyms_name";

    public long NameSynonymsId { get; init; }

    public required string Name { get; init; }

    public required string[] Synonyms { get; set; }
}

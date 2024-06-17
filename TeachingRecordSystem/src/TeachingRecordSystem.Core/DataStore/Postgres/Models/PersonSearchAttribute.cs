namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class PersonSearchAttribute
{
    public const int AttributeTypeMaxLength = 50;
    public const int AttributeValueMaxLength = 1000;
    public const int AttributeKeyMaxLength = 50;
    public const string PersonIdIndexName = "ix_person_search_attributes_person_id";
    public const string AttributeTypeAndValueIndexName = "ix_person_search_attributes_attribute_type_and_value";

    public long PersonSearchAttributeId { get; init; }

    public required Guid PersonId { get; init; }

    public required string AttributeType { get; init; }

    public required string AttributeValue { get; init; }

    public required string[] Tags { get; init; }

    public string? AttributeKey { get; init; }
}

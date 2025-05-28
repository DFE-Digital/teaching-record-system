namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class PreviousName
{
    public const string PersonIdIndexName = "ix_previous_names_person_id";
    public const string PersonForeignKeyName = "fk_previous_names_person";

    public required Guid PreviousNameId { get; init; }
    public required Guid PersonId { get; init; }
    public Person? Person { get; }
    public required DateTime? CreatedOn { get; init; }
    public required DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
}

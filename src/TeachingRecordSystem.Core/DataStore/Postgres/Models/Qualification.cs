namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public abstract class Qualification
{
    public const string PersonForeignKeyName = "fk_qualifications_person";

    public required Guid QualificationId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public QualificationType QualificationType { get; protected set; }
    public required Guid PersonId { get; init; }
    public Person? Person { get; }
}

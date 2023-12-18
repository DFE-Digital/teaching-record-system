namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public abstract class Qualification
{
    public const string PersonForeignKeyName = "fk_qualifications_person";

    public required Guid QualificationId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public QualificationType QualificationType { get; }
    public required Guid PersonId { get; init; }

    public Guid? DqtQualificationId { get; set; }
    public DateTime? DqtFirstSync { get; set; }
    public DateTime? DqtLastSync { get; set; }
    public int? DqtState { get; set; }
    public DateTime? DqtCreatedOn { get; set; }
    public DateTime? DqtModifiedOn { get; set; }
}

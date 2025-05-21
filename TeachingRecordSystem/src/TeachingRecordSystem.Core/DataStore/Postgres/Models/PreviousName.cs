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

    public Guid? DqtFirstNamePreviousNameId { get; init; }
    public DateTime? DqtFirstNameFirstSync { get; set; }
    public DateTime? DqtFirstNameLastSync { get; set; }
    public int? DqtFirstNameState { get; set; }
    public DateTime? DqtFirstNameCreatedOn { get; set; }
    public DateTime? DqtFirstNameModifiedOn { get; set; }

    public Guid? DqtMiddleNamePreviousNameId { get; init; }
    public DateTime? DqtMiddleNameFirstSync { get; set; }
    public DateTime? DqtMiddleNameLastSync { get; set; }
    public int? DqtMiddleNameState { get; set; }
    public DateTime? DqtMiddleNameCreatedOn { get; set; }
    public DateTime? DqtMiddleNameModifiedOn { get; set; }

    public Guid? DqtLastNamePreviousNameId { get; init; }
    public DateTime? DqtLastNameFirstSync { get; set; }
    public DateTime? DqtLastNameLastSync { get; set; }
    public int? DqtLastNameState { get; set; }
    public DateTime? DqtLastNameCreatedOn { get; set; }
    public DateTime? DqtLastNameModifiedOn { get; set; }

}

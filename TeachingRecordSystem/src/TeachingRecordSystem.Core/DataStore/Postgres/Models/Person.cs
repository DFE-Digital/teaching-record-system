namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Person
{
    public required Guid PersonId { get; init; }
    public required DateTime? CreatedOn { get; init; }
    public required DateTime? UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public required string? Trn { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
    public required DateOnly? DateOfBirth { get; set; }  // A few DQT records in prod have a null DOB
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }

    public Guid? DqtContactId { get; init; }
    public DateTime? DqtFirstSync { get; set; }
    public DateTime? DqtLastSync { get; set; }
    public int? DqtState { get; set; }
    public DateTime? DqtCreatedOn { get; set; }
    public DateTime? DqtModifiedOn { get; set; }
    public string? DqtFirstName { get; set; }
    public string? DqtMiddleName { get; set; }
    public string? DqtLastName { get; set; }
}

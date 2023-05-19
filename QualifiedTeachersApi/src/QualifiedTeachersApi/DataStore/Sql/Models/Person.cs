namespace QualifiedTeachersApi.DataStore.Sql.Models;

public class Person
{
    public const int TrnLength = 7;
    public const int FirstNameMaxLength = 100;
    public const int MiddleNameMaxLength = 100;
    public const int LastNameMaxLength = 100;

    public required Guid PersonId { get; init; }
    public Guid? DqtContactId { get; init; }
    public int? DqtState { get; set; }
    public required string? Trn { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public required string FirstName { get; set; }
    public required string MiddleName { get; set; }
    public required string LastName { get; set; }
}

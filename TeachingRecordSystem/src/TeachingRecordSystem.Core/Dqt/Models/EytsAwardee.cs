namespace TeachingRecordSystem.Core.Dqt.Models;

public record EytsAwardee
{
    public required Guid TeacherId { get; set; }
    public required string Trn { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EmailAddress { get; set; }
}

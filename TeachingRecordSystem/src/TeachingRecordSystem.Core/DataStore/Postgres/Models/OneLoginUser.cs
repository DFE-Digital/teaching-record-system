namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class OneLoginUser
{
    public required string Subject { get; init; }
    public required string Email { get; set; }
    public required DateTime FirstOneLoginSignIn { get; init; }
    public required DateTime LastOneLoginSignIn { get; set; }
    public DateTime? FirstSignIn { get; set; }
    public DateTime? LastSignIn { get; set; }
    public Guid? PersonId { get; set; }
    public Person? Person { get; }
}

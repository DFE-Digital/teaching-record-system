using EntityFrameworkCore.Projectables;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTask
{
    public string SupportTaskReference { get; } = null!;
    public required DateTime CreatedOn { get; init; }
    public Guid? AssignedToUserId { get; set; }
    public User? AssignedTo { get; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public User? CompletedBy { get; }
    public DateTime? DeletedOn { get; set; }
    public required SupportTaskType SupportTaskType { get; init; }
    public SupportTaskStatus Status { get; set; } = SupportTaskStatus.Open;
    public string? OneLoginUserSubject { get; init; }
    public OneLoginUser? OneLoginUser { get; }
    public Guid? PersonId { get; init; }
    public Person? Person { get; }
    public Guid? TrnRequestApplicationUserId { get; init; }
    public string? TrnRequestId { get; init; }
    public TrnRequestMetadata? TrnRequestMetadata { get; }
    public required string? SubjectName { get; init; }
    public required string? SubjectEmailAddress { get; init; }
    public required ISupportTaskData Data { get; set; }
    public SavedJourneyState? ResolveJourneySavedState { get; set; }
    public string[] ZendeskTickets { get; set; } = Array.Empty<string>();
    public SupportTaskOutcome? Outcome { get; set; }

    [Projectable]
    public bool IsOutstanding => Status != SupportTaskStatus.Closed;

    public T GetData<T>() where T : ISupportTaskData => (T)Data;

    public string GetSubject() => SubjectName ??
        SubjectEmailAddress ?? throw new InvalidOperationException($"Subject has not been set.");

    public sealed record Subject
    {
        private Subject(string? name, string? emailAddress)
        {
            if (name is null && emailAddress is null)
            {
                throw new ArgumentException("Either name or email address must be specified.");
            }

            if (name is not null && emailAddress is not null)
            {
                throw new ArgumentException("Either name or email address must be specified.");
            }

            Name = name;
            EmailAddress = emailAddress;
        }

        public string? Name { get; }

        public string? EmailAddress { get; }

        public static Subject FromPerson(Person person) => new(
            string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName),
            null);

        public static Subject FromTrnRequest(TrnRequestMetadata trnRequest) => new(
            string.JoinNonEmpty(' ', trnRequest.FirstName, trnRequest.MiddleName, trnRequest.LastName),
            null);

        public static Subject FromOneLoginUser(OneLoginUser oneLoginUser) =>
            oneLoginUser.VerifiedNames is not null
                ? FromOneLoginUser(oneLoginUser.VerifiedNames!)
                : FromOneLoginUser(oneLoginUser.EmailAddress!);

        public static Subject FromOneLoginUser(string firstName, string lastName) => new(
            string.JoinNonEmpty(' ', firstName, lastName),
            null);

        public static Subject FromOneLoginUser(string[][] verifiedNames) => new(
            string.JoinNonEmpty(' ', verifiedNames.First()),
            null);

        public static Subject FromOneLoginUser(string emailAddress) =>
            new(name: null, emailAddress);
    }
}

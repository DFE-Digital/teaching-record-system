namespace TeachingRecordSystem.Core.Events.Models;

public record OneLoginUser
{
    public required string Subject { get; init; }
    public required string? EmailAddress { get; init; }
    public required Guid? PersonId { get; init; }
    public required DateTime? VerifiedOn { get; init; }
    public required OneLoginUserVerificationRoute? VerificationRoute { get; init; }
    public required string[][]? VerifiedNames { get; init; }
    public required DateOnly[]? VerifiedDatesOfBirth { get; init; }
    public required DateTime? MatchedOn { get; init; }
    public required OneLoginUserMatchRoute? MatchRoute { get; init; }
    public required KeyValuePair<PersonMatchedAttribute, string>[]? MatchedAttributes { get; init; }
    public required Guid? VerifiedByApplicationUserId { get; init; }

    public static OneLoginUser FromModel(Core.DataStore.Postgres.Models.OneLoginUser model) => new OneLoginUser()
    {
        Subject = model.Subject,
        EmailAddress = model.EmailAddress,
        PersonId = model.PersonId,
        VerifiedOn = model.VerifiedOn,
        VerificationRoute = model.VerificationRoute,
        VerifiedNames = model.VerifiedNames,
        VerifiedDatesOfBirth = model.VerifiedDatesOfBirth,
        MatchedOn = model.MatchedOn,
        MatchRoute = model.MatchRoute,
        MatchedAttributes = model.MatchedAttributes?.ToArray(),
        VerifiedByApplicationUserId = model.VerifiedByApplicationUserId
    };
}

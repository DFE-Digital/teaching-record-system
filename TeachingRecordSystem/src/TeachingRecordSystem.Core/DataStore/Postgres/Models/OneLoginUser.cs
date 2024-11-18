using System.Diagnostics;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class OneLoginUser
{
    public required string Subject { get; init; }
    public string? Email { get; set; }
    public DateTime? FirstOneLoginSignIn { get; set; }
    public DateTime? LastOneLoginSignIn { get; set; }
    public DateTime? FirstSignIn { get; set; }
    public DateTime? LastSignIn { get; set; }
    public Guid? PersonId { get; private set; }
    public Person? Person { get; }
    public DateTime? VerifiedOn { get; private set; }
    public OneLoginUserVerificationRoute? VerificationRoute { get; private set; }
    public string[][]? VerifiedNames { get; private set; }
    public DateOnly[]? VerifiedDatesOfBirth { get; private set; }
    public string? LastCoreIdentityVc { get; set; }
    public OneLoginUserMatchRoute? MatchRoute { get; private set; }
    public KeyValuePair<OneLoginUserMatchedAttribute, string>[]? MatchedAttributes { get; private set; }
    public Guid? VerifiedByApplicationUserId { get; private set; }

    public void SetVerified(
        DateTime verifiedOn,
        OneLoginUserVerificationRoute route,
        Guid? verifiedByApplicationUserId,
        string[][]? verifiedNames,
        DateOnly[]? verifiedDatesOfBirth)
    {
        if (route == OneLoginUserVerificationRoute.External && !verifiedByApplicationUserId.HasValue)
        {
            throw new ArgumentException(
                $"{nameof(verifiedByApplicationUserId)} must be non-null when {nameof(route)} is {OneLoginUserVerificationRoute.External}.",
                nameof(verifiedByApplicationUserId));
        }

        if (route != OneLoginUserVerificationRoute.External && verifiedByApplicationUserId.HasValue)
        {
            throw new ArgumentException(
                $"{nameof(verifiedByApplicationUserId)} must be null when {nameof(route)} is not {OneLoginUserVerificationRoute.External}.",
                nameof(verifiedByApplicationUserId));
        }

        VerifiedOn = verifiedOn;
        VerificationRoute = route;
        VerifiedByApplicationUserId = verifiedByApplicationUserId;
        VerifiedNames = verifiedNames;
        VerifiedDatesOfBirth = verifiedDatesOfBirth;
    }

    public void SetMatched(
        Guid personId,
        OneLoginUserMatchRoute route,
        KeyValuePair<OneLoginUserMatchedAttribute, string>[]? matchedAttributes)
    {
        Debug.Assert(VerifiedOn is not null);

        PersonId = personId;
        MatchRoute = route;
        MatchedAttributes = matchedAttributes;
    }

    public void ClearVerifiedInfo()
    {
        // Used for testing only
        VerifiedOn = null;
        VerificationRoute = null;
        VerifiedNames = null;
        VerifiedDatesOfBirth = null;
    }

    public void ClearMatchedPerson()
    {
        // Used for testing only
        PersonId = null;
        MatchRoute = null;
        MatchedAttributes = null;
    }
}

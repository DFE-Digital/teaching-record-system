namespace TeachingRecordSystem.Core.Services.TrnRequests;

public sealed record MatchPersonsResult
{
    private readonly Guid _personId;
    private readonly string? _trn;
    private readonly PersonMatchedAttribute[] _matchedAttributes;
    private readonly MatchPersonResult[] _potentialMatches;

    private MatchPersonsResult(
        MatchPersonsResultOutcome outcome,
        Guid personId,
        string? trn,
        IEnumerable<PersonMatchedAttribute> matchedAttributes,
        IEnumerable<MatchPersonResult> potentialMatches)
    {
        Outcome = outcome;
        _personId = personId;
        _trn = trn;
        _matchedAttributes = matchedAttributes.ToArray();
        _potentialMatches = potentialMatches.ToArray();
    }

    public static MatchPersonsResult NoMatches() =>
        new(MatchPersonsResultOutcome.NoMatches, Guid.Empty, null, Array.Empty<PersonMatchedAttribute>(), Array.Empty<MatchPersonResult>());

    public static MatchPersonsResult PotentialMatches(IEnumerable<MatchPersonResult> potentialMatches) =>
        new(MatchPersonsResultOutcome.PotentialMatches, Guid.Empty, null, Array.Empty<PersonMatchedAttribute>(), potentialMatches);

    public static MatchPersonsResult DefiniteMatch(Guid personId, string trn, IEnumerable<PersonMatchedAttribute> matchedAttributes) =>
        new(MatchPersonsResultOutcome.DefiniteMatch, personId, trn, matchedAttributes, Array.Empty<MatchPersonResult>());

    public MatchPersonsResultOutcome Outcome { get; }

    public IReadOnlyCollection<MatchPersonResult> Matches
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.PotentialMatches)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.PotentialMatches)} outcome has {nameof(Matches)}.");
            }

            return _potentialMatches;
        }
    }

    public Guid PersonId
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.DefiniteMatch)} outcome has a {nameof(PersonId)}.");
            }

            return _personId;
        }
    }

    public string Trn
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.DefiniteMatch)} has a {nameof(Trn)}.");
            }

            return _trn!;
        }
    }

    public IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.DefiniteMatch)} has {nameof(MatchedAttributes)}.");
            }

            return _matchedAttributes;
        }
    }
}

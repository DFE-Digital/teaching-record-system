namespace TeachingRecordSystem.Core.Services.TrnRequests;

public sealed record MatchPersonsResult
{
    private readonly PotentialMatch? _singleMatch;
    private readonly PotentialMatch[] _potentialMatches;
    private readonly PersonMatchedAttribute[] _matchedAttributes;

    private MatchPersonsResult(
        MatchPersonsResultOutcome outcome,        
        PotentialMatch? singleMatch,
        IEnumerable<PotentialMatch> potentialMatches,
        IEnumerable<PersonMatchedAttribute> matchedAttributes)
    {
        Outcome = outcome;
        _singleMatch = singleMatch;
        _potentialMatches = potentialMatches.ToArray();
        _matchedAttributes = matchedAttributes.ToArray();
    }

    public static MatchPersonsResult NoMatches() =>
        new(MatchPersonsResultOutcome.NoMatches, null, [], []);

    public static MatchPersonsResult PotentialMatches(IEnumerable<PotentialMatch> potentialMatches) =>
        new(MatchPersonsResultOutcome.PotentialMatches, null, potentialMatches, []);

    public static MatchPersonsResult DefiniteMatch(PotentialMatch singleMatch, IEnumerable<PersonMatchedAttribute> matchedAttributes) =>
        new(MatchPersonsResultOutcome.DefiniteMatch, singleMatch, [], matchedAttributes);

    public MatchPersonsResultOutcome Outcome { get; }

    public IReadOnlyCollection<PotentialMatch> Matches
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

            return _singleMatch!.PersonId;
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

            return _singleMatch!.Trn;
        }
    }

    public PotentialMatch SingleMatch
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.DefiniteMatch)} has {nameof(SingleMatch)}.");
            }

            return _singleMatch!;
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

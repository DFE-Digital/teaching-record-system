namespace TeachingRecordSystem.Core.Services.TrnRequests;

public sealed record MatchPersonsResult
{
    private readonly Guid _personId;
    private readonly string? _trn;
    private readonly PersonMatchedAttribute[] _matchedAttributes;
    private readonly MatchPersonsResultPerson[] _potentialMatches;

    private MatchPersonsResult(
        MatchPersonsResultOutcome outcome,
        Guid personId,
        string? trn,
        IEnumerable<PersonMatchedAttribute> matchedAttributes,
        IEnumerable<MatchPersonsResultPerson> potentialMatches)
    {
        Outcome = outcome;
        _personId = personId;
        _trn = trn;
        _matchedAttributes = matchedAttributes.ToArray();
        _potentialMatches = potentialMatches.ToArray();
    }

    public static MatchPersonsResult NoMatches() =>
        new(MatchPersonsResultOutcome.NoMatches, Guid.Empty, null, Array.Empty<PersonMatchedAttribute>(), Array.Empty<MatchPersonsResultPerson>());

    public static MatchPersonsResult PotentialMatches(IEnumerable<MatchPersonsResultPerson> potentialMatches) =>
        new(MatchPersonsResultOutcome.PotentialMatches, Guid.Empty, null, Array.Empty<PersonMatchedAttribute>(), potentialMatches);

    public static MatchPersonsResult DefiniteMatch(Guid personId, string trn, IEnumerable<PersonMatchedAttribute> matchedAttributes) =>
        new(MatchPersonsResultOutcome.DefiniteMatch, personId, trn, matchedAttributes, Array.Empty<MatchPersonsResultPerson>());

    public MatchPersonsResultOutcome Outcome { get; }

    public IReadOnlyCollection<MatchPersonsResultPerson> Matches
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

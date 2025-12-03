namespace TeachingRecordSystem.Core.Services.TrnRequests;

public sealed record MatchPersonsResult
{
    private readonly Guid _personId;
    private readonly string? _trn;
    private readonly Guid[] _potentialMatchesPersonIds;

    private MatchPersonsResult(
        MatchPersonsResultOutcome outcome,
        Guid personId,
        string? trn,
        IEnumerable<Guid> potentialMatchesPersonIds)
    {
        Outcome = outcome;
        _potentialMatchesPersonIds = potentialMatchesPersonIds.ToArray();
        _personId = personId;
        _trn = trn;
    }

    public static MatchPersonsResult NoMatches() =>
        new(MatchPersonsResultOutcome.NoMatches, Guid.Empty, null, []);

    public static MatchPersonsResult PotentialMatches(IEnumerable<Guid> personIds) =>
        new(MatchPersonsResultOutcome.PotentialMatches, Guid.Empty, null, personIds);

    public static MatchPersonsResult DefiniteMatch(Guid personId, string trn) =>
        new(MatchPersonsResultOutcome.DefiniteMatch, personId, trn, []);

    public MatchPersonsResultOutcome Outcome { get; }

    public IReadOnlyCollection<Guid> PotentialMatchesPersonIds
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.PotentialMatches)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.PotentialMatches)} outcome has {nameof(PotentialMatchesPersonIds)}.");
            }

            return _potentialMatchesPersonIds;
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
}

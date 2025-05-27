namespace TeachingRecordSystem.Core.Services.PersonMatching;

public sealed record TrnRequestMatchResult
{
    private readonly Guid _personId;
    private readonly string? _trn;
    private readonly Guid[] _potentialMatchesPersonIds;

    private TrnRequestMatchResult(
        TrnRequestMatchResultOutcome outcome,
        Guid personId,
        string? trn,
        IEnumerable<Guid> potentialMatchesPersonIds)
    {
        Outcome = outcome;
        _potentialMatchesPersonIds = potentialMatchesPersonIds.ToArray();
        _personId = personId;
        _trn = trn;
    }

    public static TrnRequestMatchResult NoMatches() =>
        new(TrnRequestMatchResultOutcome.NoMatches, Guid.Empty, null, []);

    public static TrnRequestMatchResult PotentialMatches(IEnumerable<Guid> personIds) =>
        new(TrnRequestMatchResultOutcome.PotentialMatches, Guid.Empty, null, personIds);

    public static TrnRequestMatchResult DefiniteMatch(Guid personId, string trn) =>
        new(TrnRequestMatchResultOutcome.DefiniteMatch, personId, trn, []);

    public TrnRequestMatchResultOutcome Outcome { get; }

    public IReadOnlyCollection<Guid> PotentialMatchesPersonIds
    {
        get
        {
            if (Outcome != TrnRequestMatchResultOutcome.PotentialMatches)
            {
                throw new InvalidOperationException($"Only a {nameof(TrnRequestMatchResultOutcome.PotentialMatches)} outcome has {nameof(PotentialMatchesPersonIds)}.");
            }

            return _potentialMatchesPersonIds;
        }
    }

    public Guid PersonId
    {
        get
        {
            if (Outcome != TrnRequestMatchResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(TrnRequestMatchResultOutcome.DefiniteMatch)} outcome has a {nameof(PersonId)}.");
            }

            return _personId;
        }
    }

    public string Trn
    {
        get
        {
            if (Outcome != TrnRequestMatchResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(TrnRequestMatchResultOutcome.DefiniteMatch)} has a {nameof(Trn)}.");
            }

            return _trn!;
        }
    }
}

public enum TrnRequestMatchResultOutcome
{
    NoMatches = 0,
    PotentialMatches = 1,
    DefiniteMatch = 2
}

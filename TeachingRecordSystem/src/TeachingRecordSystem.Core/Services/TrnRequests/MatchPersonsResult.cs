namespace TeachingRecordSystem.Core.Services.TrnRequests;

public sealed record MatchPersonsResult
{
    private readonly PotentialMatch? _definiteMatch;
    private readonly PotentialMatch[] _potentialMatches;

    private MatchPersonsResult(
        MatchPersonsResultOutcome outcome,        
        PotentialMatch? definiteMatch,
        IEnumerable<PotentialMatch> potentialMatches)
    {
        Outcome = outcome;
        _definiteMatch = definiteMatch;
        _potentialMatches = potentialMatches.ToArray();
    }

    public static MatchPersonsResult NoMatches() =>
        new(MatchPersonsResultOutcome.NoMatches, null, []);

    public static MatchPersonsResult PotentialMatches(IEnumerable<PotentialMatch> potentialMatches) =>
        new(MatchPersonsResultOutcome.PotentialMatches, null, potentialMatches);

    public static MatchPersonsResult DefiniteMatch(PotentialMatch definiteMatch) =>
        new(MatchPersonsResultOutcome.DefiniteMatch, definiteMatch, []);

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

            return _definiteMatch!.PersonId;
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

            return _definiteMatch!.Trn;
        }
    }

    public PotentialMatch SingleDefiniteMatch
    {
        get
        {
            if (Outcome != MatchPersonsResultOutcome.DefiniteMatch)
            {
                throw new InvalidOperationException($"Only a {nameof(MatchPersonsResultOutcome.DefiniteMatch)} has {nameof(SingleDefiniteMatch)}.");
            }

            return _definiteMatch!;
        }
    }
}

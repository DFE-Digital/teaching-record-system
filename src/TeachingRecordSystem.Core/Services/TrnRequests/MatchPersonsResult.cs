namespace TeachingRecordSystem.Core.Services.TrnRequests;

public union MatchPersonsResult(MatchPersonsResult.NoMatches, MatchPersonsResult.PotentialMatches, MatchPersonsResult.DefiniteMatch)
{
    public MatchPersonsResultOutcome Outcome => this switch
    {
        NoMatches => MatchPersonsResultOutcome.NoMatches,
        PotentialMatches => MatchPersonsResultOutcome.PotentialMatches,
        DefiniteMatch => MatchPersonsResultOutcome.DefiniteMatch,
        null => throw new InvalidOperationException($"{nameof(MatchPersonsResult)} has no value.")
    };

    public sealed record NoMatches;

    public sealed record PotentialMatches(IReadOnlyCollection<MatchPersonsResultPerson> Matches);

    public sealed record DefiniteMatch(Guid PersonId, string Trn, IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes);
}

namespace TeachingRecordSystem.Api.V3.V20240920.Requests;

public record FindPersonsRequest
{
    public required IReadOnlyCollection<FindPersonsRequestPerson> Persons { get; init; }
}

public record FindPersonsRequestPerson
{
    public required string Trn { get; init; }
    public required DateOnly DateOfBirth { get; init; }
}
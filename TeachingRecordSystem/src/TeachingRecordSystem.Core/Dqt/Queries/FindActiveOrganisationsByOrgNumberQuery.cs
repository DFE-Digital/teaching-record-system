namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindActiveOrganisationsByOrgNumberQuery : ICrmQuery<Account[]>
{
    public required string OrganisationNumber { get; init; }
}

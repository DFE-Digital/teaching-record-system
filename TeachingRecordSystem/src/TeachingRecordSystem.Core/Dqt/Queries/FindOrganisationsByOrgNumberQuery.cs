namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindOrganisationsByOrgNumberQuery : ICrmQuery<Account[]>
{
    public required string OrganisationNumber { get; init; }
}

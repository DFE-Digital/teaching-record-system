namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindActiveOrganisationsByAccountNumberQuery(string AccountNumber) : ICrmQuery<Account[]>;

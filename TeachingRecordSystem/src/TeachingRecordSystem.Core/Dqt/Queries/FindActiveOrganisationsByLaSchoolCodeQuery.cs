namespace TeachingRecordSystem.Core.Dqt.Queries;

public record FindActiveOrganisationsByLaSchoolCodeQuery(string LaSchoolCode) : ICrmQuery<Account[]>;

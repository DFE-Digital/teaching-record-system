namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetOpenTasksForEmailAddressQuery(string EmailAddress) : ICrmQuery<CrmTask[]>;

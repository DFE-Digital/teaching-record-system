namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetSystemUserByAzureActiveDirectoryObjectIdQuery(string AzureActiveDirectoryObjectId) : ICrmQuery<SystemUserInfo?>;

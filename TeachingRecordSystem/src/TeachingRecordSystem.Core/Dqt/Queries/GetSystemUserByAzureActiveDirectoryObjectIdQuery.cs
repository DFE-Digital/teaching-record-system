using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetSystemUserByAzureActiveDirectoryObjectIdQuery(string AzureActiveDirectoryObjectId, ColumnSet ColumnSet) : ICrmQuery<SystemUser?>;

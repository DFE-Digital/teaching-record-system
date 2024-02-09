using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactWithParentById(Guid ContactId, ColumnSet ColumnSet) : ICrmQuery<(Contact Contact, Contact? Parent)>;

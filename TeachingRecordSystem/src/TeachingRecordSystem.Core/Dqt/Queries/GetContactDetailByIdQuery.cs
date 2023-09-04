using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetContactDetailByIdQuery(Guid ContactId, ColumnSet ColumnSet) : ICrmQuery<ContactDetail?>;

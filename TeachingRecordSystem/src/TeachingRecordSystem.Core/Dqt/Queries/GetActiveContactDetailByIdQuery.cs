using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetActiveContactDetailByIdQuery(Guid ContactId, ColumnSet ColumnSet) : ICrmQuery<ContactDetail?>;

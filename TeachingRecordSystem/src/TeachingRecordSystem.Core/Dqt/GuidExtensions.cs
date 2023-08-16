#nullable disable
using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Core.Dqt;

public static class GuidExtensions
{
    public static EntityReference ToEntityReference(this Guid id, string entityLogicalName) => new EntityReference(entityLogicalName, id);
}

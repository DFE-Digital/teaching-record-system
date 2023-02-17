using System;
using Microsoft.Xrm.Sdk;

namespace QualifiedTeachersApi.DataStore.Crm;

public static class GuidExtensions
{
    public static EntityReference ToEntityReference(this Guid id, string entityLogicalName) => new EntityReference(entityLogicalName, id);
}

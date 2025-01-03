using Microsoft.Crm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Services.TrsDataSync;

public interface IAuditRepository
{
    Task<AuditDetailCollection?> GetAuditDetailAsync(string entityLogicalName, string primaryIdAttribute, Guid id);
    Task<bool> HaveAuditDetailAsync(string entityLogicalName, Guid id);
    Task SetAuditDetailAsync(string entityLogicalName, Guid id, AuditDetailCollection auditDetailCollection);
}

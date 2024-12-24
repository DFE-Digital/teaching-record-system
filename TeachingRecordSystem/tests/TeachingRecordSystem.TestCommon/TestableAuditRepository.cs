using System.Collections.Concurrent;
using Microsoft.Crm.Sdk.Messages;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.TestCommon;

public class TestableAuditRepository : IAuditRepository
{
    private readonly ConcurrentDictionary<(string EntityLogicalName, Guid Id), AuditDetailCollection> _audits = new();

    public Task<AuditDetailCollection?> GetAuditDetailAsync(string entityLogicalName, Guid id) =>
        Task.FromResult(_audits.TryGetValue((entityLogicalName, id), out var audit) ? audit : null);

    public Task SetAuditDetailAsync(string entityLogicalName, Guid id, AuditDetailCollection auditDetailCollection)
    {
        _audits[(entityLogicalName, id)] = auditDetailCollection;
        return Task.CompletedTask;
    }
}

namespace TeachingRecordSystem.Core;

public static class DistributedLockKeys
{
    public static string EntityChanges(string changesKey, string entityLogicalName) => $"entity-changes:{changesKey}/{entityLogicalName}";
    public static string Husid(string husid) => $"husid:{husid}";
    public static string Trn(string trn) => $"trn:{trn}";
    public static string TrnRequestId(string clientId, string requestId) => $"trn-request:{clientId}/{requestId}";
}

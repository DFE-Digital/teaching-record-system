namespace QualifiedTeachersApi;

public static class DistributedLockKeys
{
    public static string Husid(string husid) => $"husid:{husid}";
    public static string Trn(string trn) => $"trn:{trn}";
    public static string TrnRequestId(string clientId, string requestId) => $"trn-request:{clientId}/{requestId}";
}

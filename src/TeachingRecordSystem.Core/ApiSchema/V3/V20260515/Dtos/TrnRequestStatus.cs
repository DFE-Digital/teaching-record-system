namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos;

public enum TrnRequestStatus
{
    Pending = 0,
    Completed = 1,
    Rejected = 2,
    Dormant = 3
}

public static class TrnRequestStatusExtensions
{
    extension(TrnRequestStatus)
    {
        public static TrnRequestStatus Create(Models.TrnRequestStatus source) => source switch
        {
            Models.TrnRequestStatus.Pending => TrnRequestStatus.Pending,
            Models.TrnRequestStatus.Completed => TrnRequestStatus.Completed,
            Models.TrnRequestStatus.Rejected => TrnRequestStatus.Rejected,
            Models.TrnRequestStatus.Dormant => TrnRequestStatus.Dormant,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}

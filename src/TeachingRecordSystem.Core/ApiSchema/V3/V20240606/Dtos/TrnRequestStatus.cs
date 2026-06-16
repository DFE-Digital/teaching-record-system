namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

public enum TrnRequestStatus
{
    Pending = 0,
    Completed = 1
}

public static class TrnRequestStatusExtensions
{
    extension(TrnRequestStatus)
    {
        public static TrnRequestStatus Create(Models.TrnRequestStatus source) => source switch
        {
            Models.TrnRequestStatus.Pending => TrnRequestStatus.Pending,
            Models.TrnRequestStatus.Completed => TrnRequestStatus.Completed,
            _ => (TrnRequestStatus)(int)source
        };
    }
}

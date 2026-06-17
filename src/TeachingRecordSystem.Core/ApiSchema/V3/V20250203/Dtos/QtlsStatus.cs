namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;

public enum QtlsStatus
{
    None = 0,
    Expired = 1,
    Active = 2
}

public static class QtlsStatusExtensions
{
    extension(QtlsStatus)
    {
        public static QtlsStatus Create(Models.QtlsStatus source) => source switch
        {
            Models.QtlsStatus.None => QtlsStatus.None,
            Models.QtlsStatus.Expired => QtlsStatus.Expired,
            Models.QtlsStatus.Active => QtlsStatus.Active,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}

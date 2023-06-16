using System.Text.Json.Serialization;
using NSwag.Examples;

namespace TeachingRecordSystem.Api.V2.Responses;

public class TrnRequestInfo
{
    public required string RequestId { get; set; }

    public TrnRequestStatus Status { get; set; }

    public required string? Trn { get; set; }

    public required bool PotentialDuplicate { get; set; }

    public required DateOnly? QtsDate { get; set; }

    [JsonIgnore]
    public bool WasCreated { get; set; }

    public string? SlugId { get; set; }
}

public class TrnRequestInfoExample : IExampleProvider<TrnRequestInfo>
{
    public TrnRequestInfo GetExample() => new()
    {
        RequestId = "72888c5d-db14-4222-829b-7db9c2ec0dc3",
        Status = TrnRequestStatus.Completed,
        Trn = "1234567",
        PotentialDuplicate = false,
        QtsDate = null
    };
}

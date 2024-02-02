using System.Text.Json.Serialization;
using Optional;

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

    public required Option<string> AccessYourTeachingQualificationsLink { get; set; }
}

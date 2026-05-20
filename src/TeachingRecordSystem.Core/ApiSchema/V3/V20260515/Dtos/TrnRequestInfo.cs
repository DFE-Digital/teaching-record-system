namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos;

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public required string? Trn { get; init; }
    public required bool PotentialDuplicate { get; init; }
    public required string? AccessYourTeachingQualificationsLink { get; init; }
}

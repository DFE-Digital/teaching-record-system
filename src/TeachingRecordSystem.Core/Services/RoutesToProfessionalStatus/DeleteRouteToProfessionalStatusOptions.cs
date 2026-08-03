namespace TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;

public record DeleteRouteToProfessionalStatusOptions
{
    public required Guid QualificationId { get; init; }
    public required EventModels.RaisedByUserInfo DeletedBy { get; init; }
    public string? DeletionReason { get; init; }
    public string? DeletionReasonDetail { get; init; }
    public EventModels.File? EvidenceFile { get; init; }
    public string? AdditionalInformation { get; init; }
}

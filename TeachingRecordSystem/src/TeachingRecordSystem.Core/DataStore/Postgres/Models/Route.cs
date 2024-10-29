namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Route
{
    public required Guid RouteId { get; init; }
    public required Guid PersonId { get; init; }
    public required string? ExternalReference { get; init; }  // e.g. Register's slug
    public required QualificationType QualificationType { get; init; }
    public Guid? QualificationId { get; set; }
    public Qualification? Qualification { get; }
    public required RouteType RouteType { get; init; }
    public required RouteStatus RouteStatus { get; set; }
    public required string? CountryId { get; set; }
    public required InductionExemptionReason? InductionExemptionReason { get; set; }
    public required Guid? IttProviderId { get; set; }
    public required DateOnly? ProgrammeStartDate { get; set; }
    public required DateOnly? ProgrammeEndDate { get; set; }
    public required int? AgeRangeFrom { get; set; }
    public required int? AgeRangeTo { get; set; }
    public required List<Guid> Subjects { get; set; } = [];
}

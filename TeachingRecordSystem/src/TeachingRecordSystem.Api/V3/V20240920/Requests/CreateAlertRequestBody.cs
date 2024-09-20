using Optional;

namespace TeachingRecordSystem.Api.V3.V20240920.Requests;

public record CreateAlertRequestBody
{
    public required string Trn { get; init; }
    public required Guid AlertTypeId { get; init; }
    public required DateOnly StartDate { get; init; }
    public Option<DateOnly?> EndDate { get; init; }
    public Option<string?> Details { get; init; }
}

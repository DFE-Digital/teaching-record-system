using Optional;

namespace TeachingRecordSystem.Api.V3.V20240920.Requests;

public record UpdateAlertRequestBody
{
    public Option<DateOnly> StartDate { get; init; }
    public Option<DateOnly?> EndDate { get; init; }
    public Option<string?> Details { get; init; }
}

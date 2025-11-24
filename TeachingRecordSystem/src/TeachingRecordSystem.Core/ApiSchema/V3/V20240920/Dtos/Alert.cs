namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public record Alert
{
    public required Guid AlertId { get; init; }
    public required AlertType AlertType { get; init; }
    public required string? Details { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }

    public static async Task<Alert> FromEventAsync(EventModels.Alert alert, ReferenceDataCache referenceDataCache)
    {
        var alertType = await referenceDataCache.GetAlertTypeByIdAsync(alert.AlertTypeId!.Value);

        return new()
        {
            AlertId = alert.AlertId,
            AlertType = new()
            {
                AlertTypeId = alertType.AlertTypeId,
                Name = alertType.Name,
                AlertCategory = new()
                {
                    AlertCategoryId = alertType.AlertCategoryId,
                    Name = alertType.AlertCategory!.Name
                }
            },
            Details = alert.Details,
            StartDate = alert.StartDate,
            EndDate = alert.EndDate
        };
    }
}

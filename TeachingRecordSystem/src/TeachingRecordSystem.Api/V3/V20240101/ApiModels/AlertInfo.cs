using TeachingRecordSystem.Api.V3.Core.SharedModels;

namespace TeachingRecordSystem.Api.V3.V20240101.ApiModels;

[AutoMap(typeof(Alert), TypeConverter = typeof(AlertInfoTypeConverter))]
public record AlertInfo
{
    public required AlertType AlertType { get; init; }
    public required string DqtSanctionCode { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}

public class AlertInfoTypeConverter : ITypeConverter<Alert, AlertInfo>
{
    public AlertInfo Convert(Alert source, AlertInfo destination, ResolutionContext context) =>
        new()
        {
            AlertType = AlertType.Prohibition,
            DqtSanctionCode = source.AlertType.DqtSanctionCode,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
        };
}

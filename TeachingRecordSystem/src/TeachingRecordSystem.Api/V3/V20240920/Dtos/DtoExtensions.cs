using CoreAlert = TeachingRecordSystem.Api.V3.Implementation.Dtos.Alert;
using CoreAlertCategory = TeachingRecordSystem.Api.V3.Implementation.Dtos.AlertCategory;
using CoreAlertType = TeachingRecordSystem.Api.V3.Implementation.Dtos.AlertType;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public static class AlertExtensions
{
    public static Alert FromModel(this CoreAlert model) => new()
    {
        AlertId = model.AlertId,
        AlertType = model.AlertType.FromModel(),
        Details = model.Details,
        StartDate = model.StartDate,
        EndDate = model.EndDate
    };
}

public static class AlertTypeExtensions
{
    public static AlertType FromModel(this CoreAlertType model) => new()
    {
        AlertTypeId = model.AlertTypeId,
        AlertCategory = model.AlertCategory.FromModel(),
        Name = model.Name
    };
}

public static class AlertCategoryExtensions
{
    public static AlertCategory FromModel(this CoreAlertCategory model) => new()
    {
        AlertCategoryId = model.AlertCategoryId,
        Name = model.Name
    };
}

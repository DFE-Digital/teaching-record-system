using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240101;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.Alert, AlertInfo>().ConvertUsing<AlertInfoTypeConverter>();
        CreateMap<Implementation.Dtos.NameInfo, NameInfo>();
        CreateMap<Implementation.Dtos.SanctionInfo, SanctionInfo>();
    }
}

public class AlertInfoTypeConverter : ITypeConverter<Implementation.Dtos.Alert, AlertInfo>
{
    public AlertInfo Convert(Implementation.Dtos.Alert source, AlertInfo destination, ResolutionContext context) =>
        new()
        {
            AlertType = AlertType.Prohibition,
            DqtSanctionCode = source.AlertType.DqtSanctionCode!,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
        };
}

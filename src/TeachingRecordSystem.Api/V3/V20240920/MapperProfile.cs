using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Operations.Common.Alert, Alert>();
        CreateMap<Operations.Common.AlertCategory, AlertCategory>();
        CreateMap<Operations.Common.AlertType, AlertType>();
    }
}

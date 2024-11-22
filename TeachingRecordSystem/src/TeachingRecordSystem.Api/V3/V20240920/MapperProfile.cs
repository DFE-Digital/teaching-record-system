using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.Alert, Alert>();
        CreateMap<Implementation.Dtos.AlertCategory, AlertCategory>();
        CreateMap<Implementation.Dtos.AlertType, AlertType>();
    }
}

using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240920;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<PostgresModels.Alert, Alert>();
        CreateMap<PostgresModels.AlertCategory, AlertCategory>();
        CreateMap<PostgresModels.AlertType, AlertType>();
    }
}

using TeachingRecordSystem.Core.ApiSchema.V3.V20250327.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250327;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.QtsInfo, QtsInfo>();
    }
}

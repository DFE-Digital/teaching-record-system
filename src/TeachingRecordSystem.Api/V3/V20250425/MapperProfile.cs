using TeachingRecordSystem.Core.ApiSchema.V3.V20250425.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250425;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
    }
}

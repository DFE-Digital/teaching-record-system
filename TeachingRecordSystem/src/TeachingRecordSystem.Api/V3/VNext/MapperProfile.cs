using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
    }
}

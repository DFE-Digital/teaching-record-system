using TeachingRecordSystem.Core.ApiSchema.V3.V20260515.Dtos;

namespace TeachingRecordSystem.Api.V3.V20260515;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
        CreateMap<Core.Models.TrnRequestStatus, Core.ApiSchema.V3.V20260515.Dtos.TrnRequestStatus>();
    }
}

using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using TrnRequestInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.TrnRequestInfo;

namespace TeachingRecordSystem.Api.V3.V20250203;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Operations.Common.TrnRequestInfo, TrnRequestInfo>();
        CreateMap<Operations.Common.InductionInfo, InductionInfo>();
    }
}

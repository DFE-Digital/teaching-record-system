using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;
using InductionStatusInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos.InductionStatusInfo;

namespace TeachingRecordSystem.Api.V3.V20240814;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.EytsInfo, EytsInfo>();
        CreateMap<Implementation.Dtos.InductionStatusInfo, InductionStatusInfo>();
        CreateMap<Implementation.Dtos.QtsInfo, QtsInfo>();
    }
}

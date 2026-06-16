using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Operations.Common.EytsInfo, EytsInfo>()
            .ForMember(i => i.Awarded, m => m.MapFrom(i => i.HoldsFrom));
        CreateMap<Operations.Common.DqtInductionStatusInfo, DqtInductionStatusInfo>();
        CreateMap<Operations.Common.QtsInfo, QtsInfo>()
            .ForMember(i => i.Awarded, m => m.MapFrom(i => i.HoldsFrom));
    }
}

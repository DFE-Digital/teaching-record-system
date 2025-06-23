using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.EytsInfo, EytsInfo>()
            .ForMember(i => i.Awarded, m => m.MapFrom(i => i.HoldsFrom));
        CreateMap<Implementation.Dtos.DqtInductionStatusInfo, DqtInductionStatusInfo>();
        CreateMap<Implementation.Dtos.QtsInfo, QtsInfo>()
            .ForMember(i => i.Awarded, m => m.MapFrom(i => i.HoldsFrom));
    }
}

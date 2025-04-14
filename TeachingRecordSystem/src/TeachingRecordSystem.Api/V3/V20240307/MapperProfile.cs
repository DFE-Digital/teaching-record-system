using TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;
#pragma warning disable TRS0001

namespace TeachingRecordSystem.Api.V3.V20240307;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
        CreateMap<Implementation.Dtos.TrnRequestInfoPerson, TrnRequestPerson>()
            .ForMember(p => p.Email, m => m.MapFrom(p => p.EmailAddress));
    }
}

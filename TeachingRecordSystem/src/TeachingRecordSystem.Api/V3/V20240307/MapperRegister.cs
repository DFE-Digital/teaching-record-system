using Mapster;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240307.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240307;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
        config.NewConfig<Implementation.Dtos.TrnRequestInfoPerson, TrnRequestPerson>()
            .Map(dest => dest.Email, src => src.EmailAddress);
    }
}

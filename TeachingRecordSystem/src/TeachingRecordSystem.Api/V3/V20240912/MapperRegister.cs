using Mapster;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240912.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240912;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Implementation.Dtos.QtlsResult, QtlsResponse>();
    }
}

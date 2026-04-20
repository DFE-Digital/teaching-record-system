using Mapster;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240814.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Implementation.Dtos.EytsInfo, EytsInfo>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<Implementation.Dtos.DqtInductionStatusInfo, DqtInductionStatusInfo>();

        config.NewConfig<Implementation.Dtos.QtsInfo, QtsInfo>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<FindPersonsResult, FindPersonsResponse>()
            .Map(dest => dest.Results, src => src.Items);

        config.NewConfig<FindPersonsResultItem, FindPersonsResponseResult>()
            .Map(dest => dest.InductionStatus, src => src.DqtInductionStatus);

        config.NewConfig<FindPersonsResultItem, FindPersonResponseResult>()
            .Map(dest => dest.InductionStatus, src => src.DqtInductionStatus);
    }
}

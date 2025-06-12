using OneOf;
using TeachingRecordSystem.Api.Infrastructure.Mapping;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240606;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
        CreateMap<OneOf<GetPersonResultInitialTeacherTraining, GetPersonResultInitialTeacherTrainingForAppropriateBody>,
                GetPersonResponseInitialTeacherTraining>()
            .ConvertUsing(
                new FromOneOfT0TypeConverter<GetPersonResultInitialTeacherTraining, GetPersonResultInitialTeacherTrainingForAppropriateBody,
                    GetPersonResponseInitialTeacherTraining>());
    }
}

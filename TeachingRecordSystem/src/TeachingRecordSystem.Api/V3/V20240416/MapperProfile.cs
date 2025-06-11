using OneOf;
using TeachingRecordSystem.Api.Infrastructure.Mapping;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240416.Responses;

namespace TeachingRecordSystem.Api.V3.V20240416;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<OneOf<GetPersonResultInitialTeacherTraining, GetPersonResultInitialTeacherTrainingForAppropriateBody>,
                GetTeacherResponseInitialTeacherTraining>()
            .ConvertUsing(
                new FromOneOfT0TypeConverter<GetPersonResultInitialTeacherTraining, GetPersonResultInitialTeacherTrainingForAppropriateBody,
                    GetTeacherResponseInitialTeacherTraining>());
    }
}

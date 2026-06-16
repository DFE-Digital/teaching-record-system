using OneOf;
using TeachingRecordSystem.Api.Infrastructure.Mapping;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240101.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240101;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Operations.Common.Alert, AlertInfo>().ConvertUsing<AlertInfoTypeConverter>();
        CreateMap<Operations.Common.NameInfo, NameInfo>();
        CreateMap<Operations.Common.SanctionInfo, SanctionInfo>();
        CreateMap<
                OneOf<
                    IReadOnlyCollection<GetPersonResultInitialTeacherTraining>,
                    IReadOnlyCollection<GetPersonResultInitialTeacherTrainingForAppropriateBody>>,
                IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>>()
            .ConvertUsing(
                new FromOneOfT0TypeConverter<
                    IReadOnlyCollection<GetPersonResultInitialTeacherTraining>,
                    IReadOnlyCollection<GetPersonResultInitialTeacherTrainingForAppropriateBody>,
                    IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>>());
    }
}

public class AlertInfoTypeConverter : ITypeConverter<Operations.Common.Alert, AlertInfo>
{
    public AlertInfo Convert(Operations.Common.Alert source, AlertInfo destination, ResolutionContext context) =>
        new()
        {
            AlertType = AlertType.Prohibition,
            DqtSanctionCode = source.AlertType.DqtSanctionCode!,
            StartDate = source.StartDate,
            EndDate = source.EndDate
        };
}

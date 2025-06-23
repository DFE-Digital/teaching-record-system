using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

namespace TeachingRecordSystem.Api.V3.VNext;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Core.DataStore.Postgres.Models.RouteToProfessionalStatusType, RouteToProfessionalStatusType>();
        CreateMap<Core.DataStore.Postgres.Models.InductionExemptionReason, InductionExemptionReason>();
        CreateMap<Implementation.Dtos.InductionInfo, InductionInfo>();
        CreateMap<Implementation.Dtos.QtsInfo, QtsInfo>();
        CreateMap<Implementation.Dtos.EytsInfo, EytsInfo>();
        CreateMap<Implementation.Dtos.TrainingAgeSpecialism, TrainingAgeSpecialism>();
    }
}

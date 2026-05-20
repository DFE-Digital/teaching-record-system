using TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

namespace TeachingRecordSystem.Api.V3.V20250627;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<PostgresModels.RouteToProfessionalStatusType, RouteToProfessionalStatusType>();
        CreateMap<PostgresModels.InductionExemptionReason, InductionExemptionReason>();
        CreateMap<PostgresModels.TrainingSubject, TrainingSubject>();
        CreateMap<PostgresModels.TrainingProvider, TrainingProvider>();
        CreateMap<PostgresModels.DegreeType, DegreeType>();
        CreateMap<Implementation.Dtos.TrainingCountry, TrainingCountry>();
        CreateMap<Implementation.Dtos.InductionInfo, InductionInfo>();
        CreateMap<Implementation.Dtos.QtsInfo, QtsInfo>();
        CreateMap<Implementation.Dtos.QtsInfoRoute, QtsInfoRoute>();
        CreateMap<Implementation.Dtos.EytsInfo, EytsInfo>();
        CreateMap<Implementation.Dtos.EytsInfoRoute, EytsInfoRoute>();
        CreateMap<Implementation.Dtos.TrainingAgeSpecialism, TrainingAgeSpecialism>();
    }
}

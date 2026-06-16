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
        CreateMap<PostgresModels.Country, TrainingCountry>();
        CreateMap<Operations.Common.InductionInfo, InductionInfo>();
        CreateMap<Operations.Common.QtsInfo, QtsInfo>();
        CreateMap<Operations.Common.QtsInfoRoute, QtsInfoRoute>();
        CreateMap<Operations.Common.EytsInfo, EytsInfo>();
        CreateMap<Operations.Common.EytsInfoRoute, EytsInfoRoute>();
        CreateMap<TrainingAgeSpecialismType, TrainingAgeSpecialism>();
    }
}

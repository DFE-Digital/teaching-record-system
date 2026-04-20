using Mapster;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250627.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;
using InductionInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.InductionInfo;

namespace TeachingRecordSystem.Api.V3.V20250627;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PostgresModels.RouteToProfessionalStatusType, RouteToProfessionalStatusType>();
        config.NewConfig<PostgresModels.InductionExemptionReason, InductionExemptionReason>();
        config.NewConfig<PostgresModels.TrainingSubject, TrainingSubject>();
        config.NewConfig<PostgresModels.TrainingProvider, TrainingProvider>();
        config.NewConfig<PostgresModels.DegreeType, DegreeType>();
        config.NewConfig<Implementation.Dtos.TrainingCountry, TrainingCountry>();
        config.NewConfig<Implementation.Dtos.InductionInfo, InductionInfo>();
        config.NewConfig<Implementation.Dtos.QtsInfo, QtsInfo>();
        config.NewConfig<Implementation.Dtos.QtsInfoRoute, QtsInfoRoute>();
        config.NewConfig<Implementation.Dtos.EytsInfo, EytsInfo>();
        config.NewConfig<Implementation.Dtos.EytsInfoRoute, EytsInfoRoute>();
        config.NewConfig<Implementation.Dtos.TrainingAgeSpecialism, TrainingAgeSpecialism>();

        config.NewConfig<FindPersonsResult, FindPersonsResponse>()
            .Map(dest => dest.Results, src => src.Items);

        config.NewConfig<FindPersonsResultItem, FindPersonsResponseResult>();
        config.NewConfig<FindPersonsResultItem, FindPersonResponseResult>();

        config.NewConfig<GetPersonResultRouteToProfessionalStatus, GetPersonResponseRouteToProfessionalStatus>();
        config.NewConfig<GetPersonResultRouteToProfessionalStatusForAppropriateBody, GetPersonResponseRouteToProfessionalStatusForAppropriateBody>();
        config.NewConfig<GetPersonResultRouteToProfessionalStatusInductionExemption, GetPersonResponseProfessionalStatusInductionExemption>();
        config.NewConfig<GetPersonResultMandatoryQualification, GetPersonResponseMandatoryQualification>();

        config.NewConfig<GetPersonResult, GetPersonResponse>()
            .Map(dest => dest.Induction, src => src.Induction.Map(x => x.Adapt<InductionInfo>()))
            .Map(dest => dest.RoutesToProfessionalStatuses, src => src.RoutesToProfessionalStatuses.Map(routes =>
                routes.Match<OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>>(
                    t0 => OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>.FromT0(
                        t0.Select(r => r.Adapt<GetPersonResponseRouteToProfessionalStatus>()).ToList().AsReadOnly()),
                    t1 => OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>.FromT1(
                        t1.Select(r => r.Adapt<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>()).ToList().AsReadOnly()))))
            .Map(dest => dest.MandatoryQualifications, src => src.MandatoryQualifications.Map(list => list.Select(mq => mq.Adapt<GetPersonResponseMandatoryQualification>()).ToList().AsReadOnly()))
            .Map(dest => dest.Alerts, src => src.Alerts.Map(list => list.Select(a => a.Adapt<Alert>()).ToList().AsReadOnly()))
            .Map(dest => dest.PreviousNames, src => src.PreviousNames.Map(list => list.Select(n => n.Adapt<NameInfo>()).ToList().AsReadOnly()));
    }
}

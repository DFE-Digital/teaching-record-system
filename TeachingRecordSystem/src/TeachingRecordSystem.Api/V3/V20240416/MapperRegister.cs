using Mapster;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240416.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240416;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<
                OneOf<
                    IReadOnlyCollection<GetPersonResultInitialTeacherTraining>,
                    IReadOnlyCollection<GetPersonResultInitialTeacherTrainingForAppropriateBody>>,
                IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>>()
            .MapWith(src => src.AsT0.Select(i => i.Adapt<GetTeacherResponseInitialTeacherTraining>()).ToList().AsReadOnly());

        config.NewConfig<Implementation.Dtos.QtsInfo, GetTeacherResponseQts>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<Implementation.Dtos.EytsInfo, GetTeacherResponseEyts>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<GetPersonResultDqtInduction, GetTeacherResponseInduction>();
        config.NewConfig<GetPersonResultDqtInductionPeriod, GetTeacherResponseInductionPeriod>();
        config.NewConfig<GetPersonResultInductionPeriodAppropriateBody, GetTeacherResponseInductionPeriodAppropriateBody>();
        config.NewConfig<GetPersonResultInitialTeacherTraining, GetTeacherResponseInitialTeacherTraining>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingQualification, GetTeacherResponseInitialTeacherTrainingQualification>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingAgeRange, GetTeacherResponseInitialTeacherTrainingAgeRange>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingProvider, GetTeacherResponseInitialTeacherTrainingProvider>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingSubject, GetTeacherResponseInitialTeacherTrainingSubject>();

        config.NewConfig<GetPersonResultMandatoryQualification, GetTeacherResponseMandatoryQualification>()
            .Map(dest => dest.Awarded, src => src.EndDate);

        config.NewConfig<GetPersonResult, GetTeacherResponse>()
            .Map(dest => dest.Email, src => src.EmailAddress)
            .Map(dest => dest.Induction, src => src.DqtInduction.Map(x => x == null ? (GetTeacherResponseInduction?)null : x.Adapt<GetTeacherResponseInduction>()))
            .Map(dest => dest.InitialTeacherTraining, src => src.InitialTeacherTraining.Map(itt => itt.AsT0.Select(i => i.Adapt<GetTeacherResponseInitialTeacherTraining>()).ToList().AsReadOnly()))
            .Map(dest => dest.MandatoryQualifications, src => src.MandatoryQualifications.Map(list => list.Select(mq => mq.Adapt<GetTeacherResponseMandatoryQualification>()).ToList().AsReadOnly()))
            .Map(dest => dest.Sanctions, src => src.Sanctions.Map(list => list.Select(s => s.Adapt<SanctionInfo>()).ToList().AsReadOnly()))
            .Map(dest => dest.Alerts, src => src.Alerts.Map(list => list.Select(a => a.Adapt<AlertInfo>()).ToList().AsReadOnly()))
            .Map(dest => dest.PreviousNames, src => src.PreviousNames.Map(list => list.Select(n => n.Adapt<NameInfo>()).ToList().AsReadOnly()));
    }
}

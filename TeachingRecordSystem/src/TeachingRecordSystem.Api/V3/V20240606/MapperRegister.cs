using Mapster;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240606.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240606.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240606;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();

        config.NewConfig<
                OneOf<
                    IReadOnlyCollection<GetPersonResultInitialTeacherTraining>,
                    IReadOnlyCollection<GetPersonResultInitialTeacherTrainingForAppropriateBody>>,
                IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>>()
            .MapWith(src => src.AsT0.Select(i => i.Adapt<GetPersonResponseInitialTeacherTraining>()).ToList().AsReadOnly());

        config.NewConfig<Implementation.Dtos.QtsInfo, GetPersonResponseQts>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<Implementation.Dtos.EytsInfo, GetPersonResponseEyts>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<GetPersonResultDqtInduction, GetPersonResponseInduction>();
        config.NewConfig<GetPersonResultDqtInductionPeriod, GetPersonResponseInductionPeriod>();
        config.NewConfig<GetPersonResultInductionPeriodAppropriateBody, GetPersonResponseInductionPeriodAppropriateBody>();
        config.NewConfig<GetPersonResultInitialTeacherTraining, GetPersonResponseInitialTeacherTraining>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingQualification, GetPersonResponseInitialTeacherTrainingQualification>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingAgeRange, GetPersonResponseInitialTeacherTrainingAgeRange>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingProvider, GetPersonResponseInitialTeacherTrainingProvider>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingSubject, GetPersonResponseInitialTeacherTrainingSubject>();

        config.NewConfig<GetPersonResultMandatoryQualification, GetPersonResponseMandatoryQualification>()
            .Map(dest => dest.Awarded, src => src.EndDate);

        config.NewConfig<GetPersonResult, GetPersonResponse>()
            .Map(dest => dest.Induction, src => src.DqtInduction.Map(x => x == null ? (GetPersonResponseInduction?)null : x.Adapt<GetPersonResponseInduction>()))
            .Map(dest => dest.InitialTeacherTraining, src => src.InitialTeacherTraining.Map(itt => itt.AsT0.Select(i => i.Adapt<GetPersonResponseInitialTeacherTraining>()).ToList().AsReadOnly()))
            .Map(dest => dest.MandatoryQualifications, src => src.MandatoryQualifications.Map(list => list.Select(mq => mq.Adapt<GetPersonResponseMandatoryQualification>()).ToList().AsReadOnly()))
            .Map(dest => dest.Sanctions, src => src.Sanctions.Map(list => list.Select(s => s.Adapt<SanctionInfo>()).ToList().AsReadOnly()))
            .Map(dest => dest.Alerts, src => src.Alerts.Map(list => list.Select(a => a.Adapt<AlertInfo>()).ToList().AsReadOnly()))
            .Map(dest => dest.PreviousNames, src => src.PreviousNames.Map(list => list.Select(n => n.Adapt<NameInfo>()).ToList().AsReadOnly()));
    }
}

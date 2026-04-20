using Mapster;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20250203.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using InductionInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.InductionInfo;
using TrnRequestInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.TrnRequestInfo;

namespace TeachingRecordSystem.Api.V3.V20250203;

public class MapperRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Implementation.Dtos.TrnRequestInfo, TrnRequestInfo>();
        config.NewConfig<Implementation.Dtos.InductionInfo, InductionInfo>();

        config.NewConfig<FindPersonsResult, FindPersonsResponse>()
            .Map(dest => dest.Results, src => src.Items);

        config.NewConfig<FindPersonsResultItem, FindPersonsResponseResult>()
            .Map(dest => dest.InductionStatus, src => src.Induction.Status)
            .Map(dest => dest.QtlsStatus, src => src.QtlsStatus);

        config.NewConfig<FindPersonsResultItem, FindPersonResponseResult>()
            .Map(dest => dest.InductionStatus, src => src.Induction.Status)
            .Map(dest => dest.QtlsStatus, src => src.QtlsStatus);

        config.NewConfig<Implementation.Dtos.QtsInfo, GetPersonResponseQts>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<Implementation.Dtos.EytsInfo, GetPersonResponseEyts>()
            .Map(dest => dest.Awarded, src => src.HoldsFrom);

        config.NewConfig<GetPersonResultInitialTeacherTraining, GetPersonResponseInitialTeacherTraining>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingForAppropriateBody, GetPersonResponseInitialTeacherTrainingForAppropriateBody>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingQualification, GetPersonResponseInitialTeacherTrainingQualification>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingAgeRange, GetPersonResponseInitialTeacherTrainingAgeRange>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingProvider, GetPersonResponseInitialTeacherTrainingProvider>();
        config.NewConfig<GetPersonResultInitialTeacherTrainingSubject, GetPersonResponseInitialTeacherTrainingSubject>();

        config.NewConfig<GetPersonResultMandatoryQualification, GetPersonResponseMandatoryQualification>()
            .Map(dest => dest.Awarded, src => src.EndDate);

        config.NewConfig<GetPersonResult, GetPersonResponse>()
            .Map(dest => dest.Induction, src => src.Induction.Map(x => x.Adapt<InductionInfo>()))
            .Map(dest => dest.InitialTeacherTraining, src => src.InitialTeacherTraining.Map(itt =>
                itt.Match<OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>>(
                    t0 => OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>.FromT0(
                        t0.Select(i => i.Adapt<GetPersonResponseInitialTeacherTraining>()).ToList().AsReadOnly()),
                    t1 => OneOf<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>, IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingForAppropriateBody>>.FromT1(
                        t1.Select(i => i.Adapt<GetPersonResponseInitialTeacherTrainingForAppropriateBody>()).ToList().AsReadOnly()))))
            .Map(dest => dest.MandatoryQualifications, src => src.MandatoryQualifications.Map(list => list.Select(mq => mq.Adapt<GetPersonResponseMandatoryQualification>()).ToList().AsReadOnly()))
            .Map(dest => dest.Sanctions, src => src.Sanctions.Map(list => list.Select(s => s.Adapt<SanctionInfo>()).ToList().AsReadOnly()))
            .Map(dest => dest.Alerts, src => src.Alerts.Map(list => list.Select(a => a.Adapt<Alert>()).ToList().AsReadOnly()))
            .Map(dest => dest.PreviousNames, src => src.PreviousNames.Map(list => list.Select(n => n.Adapt<NameInfo>()).ToList().AsReadOnly()));
    }
}

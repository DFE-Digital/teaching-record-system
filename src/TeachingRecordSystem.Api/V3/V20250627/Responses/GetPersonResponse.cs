using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Operations;
using TeachingRecordSystem.Api.V3.V20240101;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos;
using TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;
using Alert = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos.Alert;
using EytsInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.EytsInfo;
using InductionInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.InductionInfo;
using NameInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.NameInfo;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.QtsInfo;
using RouteToProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos.RouteToProfessionalStatusStatus;

namespace TeachingRecordSystem.Api.V3.V20250627.Responses;

public record GetPersonResponse
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Option<bool> PendingNameChange { get; init; }
    public required Option<bool> PendingDateOfBirthChange { get; init; }
    public required string? EmailAddress { get; init; }
    public required QtsInfo? Qts { get; init; }
    public required EytsInfo? Eyts { get; init; }
    public required Option<InductionInfo> Induction { get; init; }
    public required Option<OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>> RoutesToProfessionalStatuses { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<Alert>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }
    public required QtlsStatus QtlsStatus { get; init; }

    public static GetPersonResponse Create(GetPersonResult source) => new()
    {
        Trn = source.Trn,
        FirstName = source.FirstName,
        MiddleName = source.MiddleName,
        LastName = source.LastName,
        DateOfBirth = source.DateOfBirth,
        NationalInsuranceNumber = source.NationalInsuranceNumber,
        PendingNameChange = source.PendingNameChange,
        PendingDateOfBirthChange = source.PendingDateOfBirthChange,
        EmailAddress = source.EmailAddress,
        Qts = source.Qts is { } qts ? QtsInfo.Create(qts) : null,
        Eyts = source.Eyts is { } eyts ? EytsInfo.Create(eyts) : null,
        Induction = source.Induction.Map(i => InductionInfo.Create(i)),
        RoutesToProfessionalStatuses = source.RoutesToProfessionalStatuses.Map(routes => routes.Match(
            holds => OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>.FromT0(
                holds.Select(r => GetPersonResponseRouteToProfessionalStatus.Create(r)).AsReadOnly()),
            forAppropriateBody => OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>.FromT1(
                forAppropriateBody.Select(r => GetPersonResponseRouteToProfessionalStatusForAppropriateBody.Create(r)).AsReadOnly()))),
        MandatoryQualifications = source.MandatoryQualifications.Map(mqs =>
            mqs.Select(mq => GetPersonResponseMandatoryQualification.Create(mq)).AsReadOnly()),
        Alerts = source.Alerts.Map(alerts => alerts.Select(a => Alert.Create(a)).AsReadOnly()),
        PreviousNames = source.PreviousNames.Map(names => names.Select(n => NameInfo.Create(n)).AsReadOnly()),
        AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions,
        QtlsStatus = QtlsStatus.Create(source.QtlsStatus)
    };
}

public record GetPersonResponseMandatoryQualification
{
    public required Guid MandatoryQualificationId { get; init; }
    public required DateOnly EndDate { get; init; }
    public required string Specialism { get; init; }

    public static GetPersonResponseMandatoryQualification Create(GetPersonResultMandatoryQualification source) => new()
    {
        MandatoryQualificationId = source.MandatoryQualificationId,
        EndDate = source.EndDate,
        Specialism = source.Specialism
    };
}

public record GetPersonResponseRouteToProfessionalStatus
{
    public required Guid RouteToProfessionalStatusId { get; init; }
    public required RouteToProfessionalStatusType RouteToProfessionalStatusType { get; init; }
    public required RouteToProfessionalStatusStatus Status { get; init; }
    public required DateOnly? HoldsFrom { get; init; }
    public required DateOnly? TrainingStartDate { get; init; }
    public required DateOnly? TrainingEndDate { get; init; }
    public required IReadOnlyCollection<TrainingSubject> TrainingSubjects { get; init; }
    public required TrainingAgeSpecialism? TrainingAgeSpecialism { get; init; }
    public required TrainingCountry? TrainingCountry { get; init; }
    public required TrainingProvider? TrainingProvider { get; init; }
    public required DegreeType? DegreeType { get; init; }
    public required GetPersonResponseProfessionalStatusInductionExemption InductionExemption { get; init; }

    public static GetPersonResponseRouteToProfessionalStatus Create(GetPersonResultRouteToProfessionalStatus source) => new()
    {
        RouteToProfessionalStatusId = source.RouteToProfessionalStatusId,
        RouteToProfessionalStatusType = RouteToProfessionalStatusType.Create(source.RouteToProfessionalStatusType),
        Status = RouteToProfessionalStatusStatus.Create(source.Status),
        HoldsFrom = source.HoldsFrom,
        TrainingStartDate = source.TrainingStartDate,
        TrainingEndDate = source.TrainingEndDate,
        TrainingSubjects = source.TrainingSubjects.Select(s => TrainingSubject.Create(s)).AsReadOnly(),
        TrainingAgeSpecialism = source.TrainingAgeSpecialism is { } ageSpecialism
            ? TrainingAgeSpecialism.Create(ageSpecialism, source.TrainingAgeSpecialismRangeFrom, source.TrainingAgeSpecialismRangeTo)
            : null,
        TrainingCountry = source.TrainingCountry is { } country ? TrainingCountry.Create(country) : null,
        TrainingProvider = source.TrainingProvider is { } provider ? TrainingProvider.Create(provider) : null,
        DegreeType = source.DegreeType is { } degreeType ? DegreeType.Create(degreeType) : null,
        InductionExemption = GetPersonResponseProfessionalStatusInductionExemption.Create(source.InductionExemption)
    };
}

public record GetPersonResponseRouteToProfessionalStatusForAppropriateBody
{
    public required TrainingProvider TrainingProvider { get; init; }

    public static GetPersonResponseRouteToProfessionalStatusForAppropriateBody Create(GetPersonResultRouteToProfessionalStatusForAppropriateBody source) => new()
    {
        TrainingProvider = TrainingProvider.Create(source.TrainingProvider)
    };
}

public record GetPersonResponseProfessionalStatusInductionExemption
{
    public required bool IsExempt { get; init; }
    public required IReadOnlyCollection<InductionExemptionReason> ExemptionReasons { get; init; }

    public static GetPersonResponseProfessionalStatusInductionExemption Create(GetPersonResultRouteToProfessionalStatusInductionExemption source) => new()
    {
        IsExempt = source.IsExempt,
        ExemptionReasons = source.ExemptionReasons.Select(r => InductionExemptionReason.Create(r)).AsReadOnly()
    };
}

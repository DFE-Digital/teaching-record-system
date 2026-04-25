using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;
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

    public static GetPersonResponse FromModel(GetPersonResult r) => new()
    {
        Trn = r.Trn,
        FirstName = r.FirstName,
        MiddleName = r.MiddleName,
        LastName = r.LastName,
        DateOfBirth = r.DateOfBirth,
        NationalInsuranceNumber = r.NationalInsuranceNumber,
        PendingNameChange = r.PendingNameChange,
        PendingDateOfBirthChange = r.PendingDateOfBirthChange,
        EmailAddress = r.EmailAddress,
        Qts = r.Qts.FromModel(),
        Eyts = r.Eyts.FromModel(),
        Induction = r.Induction.Map(i => i.FromModel()!),
        RoutesToProfessionalStatuses = r.RoutesToProfessionalStatuses.Map(routes => routes.IsT0
            ? OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>
                .FromT0(routes.AsT0.Select(GetPersonResponseRouteToProfessionalStatus.FromModel).ToArray())
            : OneOf<IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatus>, IReadOnlyCollection<GetPersonResponseRouteToProfessionalStatusForAppropriateBody>>
                .FromT1(routes.AsT1.Select(r2 => new GetPersonResponseRouteToProfessionalStatusForAppropriateBody
                { TrainingProvider = r2.TrainingProvider.FromModel()! }).ToArray())),
        MandatoryQualifications = r.MandatoryQualifications.MapItems(mq => new GetPersonResponseMandatoryQualification
        { MandatoryQualificationId = mq.MandatoryQualificationId, EndDate = mq.EndDate, Specialism = mq.Specialism }),
        Alerts = r.Alerts.MapItems(a => a.FromModel()),
        PreviousNames = r.PreviousNames.MapItems(n => new NameInfo { FirstName = n.FirstName, MiddleName = n.MiddleName, LastName = n.LastName }),
        AllowIdSignInWithProhibitions = r.AllowIdSignInWithProhibitions,
        QtlsStatus = (QtlsStatus)(int)r.QtlsStatus
    };
}

public record GetPersonResponseMandatoryQualification
{
    public required Guid MandatoryQualificationId { get; init; }
    public required DateOnly EndDate { get; init; }
    public required string Specialism { get; init; }
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

    public static GetPersonResponseRouteToProfessionalStatus FromModel(GetPersonResultRouteToProfessionalStatus m) => new()
    {
        RouteToProfessionalStatusId = m.RouteToProfessionalStatusId,
        RouteToProfessionalStatusType = m.RouteToProfessionalStatusType.FromModel(),
        Status = (RouteToProfessionalStatusStatus)(int)m.Status,
        HoldsFrom = m.HoldsFrom,
        TrainingStartDate = m.TrainingStartDate,
        TrainingEndDate = m.TrainingEndDate,
        TrainingSubjects = m.TrainingSubjects.Select(s => s.FromModel()).ToArray(),
        TrainingAgeSpecialism = m.TrainingAgeSpecialism.FromModel(),
        TrainingCountry = m.TrainingCountry.FromModel(),
        TrainingProvider = m.TrainingProvider.FromModel(),
        DegreeType = m.DegreeType.FromModel(),
        InductionExemption = new GetPersonResponseProfessionalStatusInductionExemption
        {
            IsExempt = m.InductionExemption.IsExempt,
            ExemptionReasons = m.InductionExemption.ExemptionReasons.Select(r => r.FromModel()).ToArray()
        }
    };
}

public record GetPersonResponseRouteToProfessionalStatusForAppropriateBody
{
    public required TrainingProvider TrainingProvider { get; init; }
}

public record GetPersonResponseProfessionalStatusInductionExemption
{
    public required bool IsExempt { get; init; }
    public required IReadOnlyCollection<InductionExemptionReason> ExemptionReasons { get; init; }
}

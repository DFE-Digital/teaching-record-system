using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;
using Alert = TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos.Alert;
using EytsInfo = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.EytsInfo;
using InductionInfo = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.InductionInfo;
using NameInfo = TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos.NameInfo;
using QtlsStatus = TeachingRecordSystem.Core.ApiSchema.V3.V20250203.Dtos.QtlsStatus;
using QtsInfo = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.QtsInfo;
using RouteToProfessionalStatusStatus = TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos.RouteToProfessionalStatusStatus;

namespace TeachingRecordSystem.Api.V3.VNext.Responses;

[AutoMap(typeof(GetPersonResult))]
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
}

[AutoMap(typeof(GetPersonResultMandatoryQualification))]
public record GetPersonResponseMandatoryQualification
{
    public required Guid MandatoryQualificationId { get; init; }
    public required DateOnly EndDate { get; init; }
    public required string Specialism { get; init; }
}

[AutoMap(typeof(GetPersonResultRouteToProfessionalStatus))]
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
}

[AutoMap(typeof(GetPersonResultRouteToProfessionalStatusForAppropriateBody))]
public record GetPersonResponseRouteToProfessionalStatusForAppropriateBody
{
    public required TrainingProvider TrainingProvider { get; init; }
}

[AutoMap(typeof(GetPersonResultRouteToProfessionalStatusInductionExemption))]
public record GetPersonResponseProfessionalStatusInductionExemption
{
    public required bool IsExempt { get; init; }
    public required IReadOnlyCollection<InductionExemptionReason> ExemptionReasons { get; init; }
}

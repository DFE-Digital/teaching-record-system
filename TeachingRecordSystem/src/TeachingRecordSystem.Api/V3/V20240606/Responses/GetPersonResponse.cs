using System.Text.Json.Serialization;
using OneOf;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240606.Responses;

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
    public required GetPersonResponseQts? Qts { get; init; }
    public required GetPersonResponseEyts? Eyts { get; init; }
    public required Option<GetPersonResponseInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<AlertInfo>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }

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
        Qts = r.Qts is not null ? new GetPersonResponseQts { Awarded = r.Qts.HoldsFrom, CertificateUrl = r.Qts.CertificateUrl, StatusDescription = r.Qts.StatusDescription } : null,
        Eyts = r.Eyts is not null ? new GetPersonResponseEyts { Awarded = r.Eyts.HoldsFrom, CertificateUrl = r.Eyts.CertificateUrl, StatusDescription = r.Eyts.StatusDescription } : null,
        Induction = r.DqtInduction.Map(i => i is not null ? GetPersonResponseInduction.FromModel(i) : null),
        InitialTeacherTraining = r.InitialTeacherTraining.Map(itt => (IReadOnlyCollection<GetPersonResponseInitialTeacherTraining>)itt.AsT0.Select(GetPersonResponseInitialTeacherTraining.FromModel).ToArray()),
        NpqQualifications = Option.None<IReadOnlyCollection<GetPersonResponseNpqQualification>>(),
        MandatoryQualifications = r.MandatoryQualifications.MapItems(mq => new GetPersonResponseMandatoryQualification { Awarded = mq.EndDate, Specialism = mq.Specialism }),
        HigherEducationQualifications = Option.None<IReadOnlyCollection<GetPersonResponseHigherEducationQualification>>(),
        Sanctions = r.Sanctions.MapItems(s => new SanctionInfo { Code = s.Code, StartDate = s.StartDate }),
        Alerts = r.Alerts.MapItems(a => new AlertInfo { AlertType = AlertType.Prohibition, DqtSanctionCode = a.AlertType.DqtSanctionCode!, StartDate = a.StartDate, EndDate = a.EndDate }),
        PreviousNames = r.PreviousNames.MapItems(n => new NameInfo { FirstName = n.FirstName, MiddleName = n.MiddleName, LastName = n.LastName }),
        AllowIdSignInWithProhibitions = r.AllowIdSignInWithProhibitions
    };
}

public record GetPersonResponseQts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

public record GetPersonResponseEyts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

public record GetPersonResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DqtInductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetPersonResponseInductionPeriod> Periods { get; init; }

    public static GetPersonResponseInduction FromModel(GetPersonResultDqtInduction m) => new()
    {
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Status = (DqtInductionStatus)(int)m.Status,
        StatusDescription = m.StatusDescription,
        CertificateUrl = m.CertificateUrl,
        Periods = m.Periods.Select(p => new GetPersonResponseInductionPeriod
        {
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Terms = p.Terms,
            AppropriateBody = p.AppropriateBody is not null ? new GetPersonResponseInductionPeriodAppropriateBody { Name = p.AppropriateBody.Name } : null
        }).ToArray()
    };
}

public record GetPersonResponseInductionPeriod
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required int? Terms { get; init; }
    public required GetPersonResponseInductionPeriodAppropriateBody? AppropriateBody { get; init; }
}

public record GetPersonResponseInductionPeriodAppropriateBody
{
    public required string Name { get; init; }
}

public record GetPersonResponseInitialTeacherTraining
{
    public required GetPersonResponseInitialTeacherTrainingQualification? Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required string? ProgrammeTypeDescription { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetPersonResponseInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required GetPersonResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required IReadOnlyCollection<GetPersonResponseInitialTeacherTrainingSubject> Subjects { get; init; }

    public static GetPersonResponseInitialTeacherTraining FromModel(GetPersonResultInitialTeacherTraining m) => new()
    {
        Qualification = m.Qualification is not null ? new GetPersonResponseInitialTeacherTrainingQualification { Name = m.Qualification.Name } : null,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        ProgrammeType = null,
        ProgrammeTypeDescription = null,
        Result = null,
        AgeRange = m.AgeRange is not null ? new GetPersonResponseInitialTeacherTrainingAgeRange { Description = m.AgeRange.Description } : null,
        Provider = m.Provider is not null ? new GetPersonResponseInitialTeacherTrainingProvider { Name = m.Provider.Name, Ukprn = m.Provider.Ukprn } : null,
        Subjects = m.Subjects.Select(s => new GetPersonResponseInitialTeacherTrainingSubject { Code = s.Code, Name = s.Name }).ToArray()
    };
}

public record GetPersonResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }
}

public record GetPersonResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

public record GetPersonResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }
}

public record GetPersonResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

public record GetPersonResponseNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetPersonResponseNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

public record GetPersonResponseNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

public record GetPersonResponseMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

public record GetPersonResponseHigherEducationQualification
{
    public required string? Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IReadOnlyCollection<GetPersonResponseHigherEducationQualificationSubject> Subjects { get; init; }
}

public record GetPersonResponseHigherEducationQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

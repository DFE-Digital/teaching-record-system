using System.Text.Json.Serialization;
using Optional;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240416.Responses;

public record GetTeacherResponse
{
    public required string Trn { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Option<bool> PendingNameChange { get; init; }
    public required Option<bool> PendingDateOfBirthChange { get; init; }
    public required string? Email { get; init; }
    public required GetTeacherResponseQts? Qts { get; init; }
    public required GetTeacherResponseEyts? Eyts { get; init; }
    public required Option<GetTeacherResponseInduction?> Induction { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>> InitialTeacherTraining { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseNpqQualification>> NpqQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseMandatoryQualification>> MandatoryQualifications { get; init; }
    public required Option<IReadOnlyCollection<GetTeacherResponseHigherEducationQualification>> HigherEducationQualifications { get; init; }
    public required Option<IReadOnlyCollection<SanctionInfo>> Sanctions { get; init; }
    public required Option<IReadOnlyCollection<AlertInfo>> Alerts { get; init; }
    public required Option<IReadOnlyCollection<NameInfo>> PreviousNames { get; init; }
    public required Option<bool> AllowIdSignInWithProhibitions { get; init; }

    public static GetTeacherResponse FromModel(GetPersonResult r) => new()
    {
        Trn = r.Trn,
        FirstName = r.FirstName,
        MiddleName = r.MiddleName,
        LastName = r.LastName,
        DateOfBirth = r.DateOfBirth,
        NationalInsuranceNumber = r.NationalInsuranceNumber,
        PendingNameChange = r.PendingNameChange,
        PendingDateOfBirthChange = r.PendingDateOfBirthChange,
        Email = r.EmailAddress,
        Qts = r.Qts is not null ? new GetTeacherResponseQts { Awarded = r.Qts.HoldsFrom, CertificateUrl = r.Qts.CertificateUrl, StatusDescription = r.Qts.StatusDescription } : null,
        Eyts = r.Eyts is not null ? new GetTeacherResponseEyts { Awarded = r.Eyts.HoldsFrom, CertificateUrl = r.Eyts.CertificateUrl, StatusDescription = r.Eyts.StatusDescription } : null,
        Induction = r.DqtInduction.Map(i => i is not null ? GetTeacherResponseInduction.FromModel(i) : null),
        InitialTeacherTraining = r.InitialTeacherTraining.Map(itt =>
            (IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>)itt.AsT0
                .Select(GetTeacherResponseInitialTeacherTraining.FromModel).ToArray()),
        NpqQualifications = Option.None<IReadOnlyCollection<GetTeacherResponseNpqQualification>>(),
        MandatoryQualifications = r.MandatoryQualifications.MapItems(mq => new GetTeacherResponseMandatoryQualification { Awarded = mq.EndDate, Specialism = mq.Specialism }),
        HigherEducationQualifications = Option.None<IReadOnlyCollection<GetTeacherResponseHigherEducationQualification>>(),
        Sanctions = r.Sanctions.MapItems(s => new SanctionInfo { Code = s.Code, StartDate = s.StartDate }),
        Alerts = r.Alerts.MapItems(a => new AlertInfo { AlertType = AlertType.Prohibition, DqtSanctionCode = a.AlertType.DqtSanctionCode!, StartDate = a.StartDate, EndDate = a.EndDate }),
        PreviousNames = r.PreviousNames.MapItems(n => new NameInfo { FirstName = n.FirstName, MiddleName = n.MiddleName, LastName = n.LastName }),
        AllowIdSignInWithProhibitions = r.AllowIdSignInWithProhibitions
    };
}

public record GetTeacherResponseQts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

public record GetTeacherResponseEyts
{
    public required DateOnly? Awarded { get; init; }
    public required string CertificateUrl { get; init; }
    public required string? StatusDescription { get; init; }
}

public record GetTeacherResponseInduction
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required DqtInductionStatus? Status { get; init; }
    public required string? StatusDescription { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string? CertificateUrl { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseInductionPeriod> Periods { get; init; }

    public static GetTeacherResponseInduction FromModel(GetPersonResultDqtInduction m) => new()
    {
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        Status = (DqtInductionStatus)(int)m.Status,
        StatusDescription = m.StatusDescription,
        CertificateUrl = m.CertificateUrl,
        Periods = m.Periods.Select(p => new GetTeacherResponseInductionPeriod
        {
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Terms = p.Terms,
            AppropriateBody = p.AppropriateBody is not null
                ? new GetTeacherResponseInductionPeriodAppropriateBody { Name = p.AppropriateBody.Name }
                : null
        }).ToArray()
    };
}

public record GetTeacherResponseInductionPeriod
{
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required int? Terms { get; init; }
    public required GetTeacherResponseInductionPeriodAppropriateBody? AppropriateBody { get; init; }
}

public record GetTeacherResponseInductionPeriodAppropriateBody
{
    public required string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTraining
{
    public required GetTeacherResponseInitialTeacherTrainingQualification? Qualification { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
    public required IttProgrammeType? ProgrammeType { get; init; }
    public required string? ProgrammeTypeDescription { get; init; }
    public required IttOutcome? Result { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingAgeRange? AgeRange { get; init; }
    public required GetTeacherResponseInitialTeacherTrainingProvider? Provider { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseInitialTeacherTrainingSubject> Subjects { get; init; }

    public static GetTeacherResponseInitialTeacherTraining FromModel(GetPersonResultInitialTeacherTraining m) => new()
    {
        Qualification = m.Qualification is not null
            ? new GetTeacherResponseInitialTeacherTrainingQualification { Name = m.Qualification.Name }
            : null,
        StartDate = m.StartDate,
        EndDate = m.EndDate,
        ProgrammeType = null,
        ProgrammeTypeDescription = null,
        Result = null,
        AgeRange = m.AgeRange is not null
            ? new GetTeacherResponseInitialTeacherTrainingAgeRange { Description = m.AgeRange.Description }
            : null,
        Provider = m.Provider is not null
            ? new GetTeacherResponseInitialTeacherTrainingProvider { Name = m.Provider.Name, Ukprn = m.Provider.Ukprn }
            : null,
        Subjects = m.Subjects.Select(s => new GetTeacherResponseInitialTeacherTrainingSubject { Code = s.Code, Name = s.Name }).ToArray()
    };
}

public record GetTeacherResponseInitialTeacherTrainingQualification
{
    public required string Name { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingAgeRange
{
    public required string Description { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingProvider
{
    public required string Name { get; init; }
    public required string Ukprn { get; init; }
}

public record GetTeacherResponseInitialTeacherTrainingSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

public record GetTeacherResponseNpqQualification
{
    public required DateOnly Awarded { get; init; }
    public required GetTeacherResponseNpqQualificationType Type { get; init; }
    public required string CertificateUrl { get; init; }
}

public record GetTeacherResponseNpqQualificationType
{
    public required NpqQualificationType Code { get; init; }
    public required string Name { get; init; }
}

public record GetTeacherResponseMandatoryQualification
{
    public required DateOnly Awarded { get; init; }
    public required string Specialism { get; init; }
}

public record GetTeacherResponseHigherEducationQualification
{
    public required string? Name { get; init; }
    public required DateOnly? Awarded { get; init; }
    public required IReadOnlyCollection<GetTeacherResponseHigherEducationQualificationSubject> Subjects { get; init; }
}

public record GetTeacherResponseHigherEducationQualificationSubject
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}

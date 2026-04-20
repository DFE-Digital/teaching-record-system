using Optional;
using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240416.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240416;

[Mapper]
public partial class ApiMapper
{
    public GetTeacherResponse MapGetTeacherResponse(GetPersonResult source) =>
        new()
        {
            Trn = source.Trn,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            DateOfBirth = source.DateOfBirth,
            NationalInsuranceNumber = source.NationalInsuranceNumber,
            PendingNameChange = source.PendingNameChange,
            PendingDateOfBirthChange = source.PendingDateOfBirthChange,
            Email = source.EmailAddress,
            Qts = source.Qts is { } qts ? MapQts(qts) : null,
            Eyts = source.Eyts is { } eyts ? MapEyts(eyts) : null,
            Induction = source.DqtInduction.Map(d => d is null ? (GetTeacherResponseInduction?)null : MapDqtInduction(d)),
            InitialTeacherTraining = source.InitialTeacherTraining.Map(
                itt => (IReadOnlyCollection<GetTeacherResponseInitialTeacherTraining>)itt.AsT0
                    .Select(MapInitialTeacherTraining).AsReadOnly()),
            NpqQualifications = Option.None<IReadOnlyCollection<GetTeacherResponseNpqQualification>>(),
            MandatoryQualifications = source.MandatoryQualifications.Map(
                mqs => (IReadOnlyCollection<GetTeacherResponseMandatoryQualification>)mqs
                    .Select(mq => new GetTeacherResponseMandatoryQualification
                    {
                        Awarded = default,
                        Specialism = mq.Specialism
                    }).AsReadOnly()),
            HigherEducationQualifications = Option.None<IReadOnlyCollection<GetTeacherResponseHigherEducationQualification>>(),
            Sanctions = source.Sanctions.Map(
                ss => (IReadOnlyCollection<SanctionInfo>)ss.Select(MapSanctionInfo).AsReadOnly()),
            Alerts = source.Alerts.Map(
                alerts => (IReadOnlyCollection<AlertInfo>)alerts.Select(MapAlertInfo).AsReadOnly()),
            PreviousNames = source.PreviousNames.Map(
                pns => (IReadOnlyCollection<NameInfo>)pns.Select(MapNameInfo).AsReadOnly()),
            AllowIdSignInWithProhibitions = source.AllowIdSignInWithProhibitions
        };

    private GetTeacherResponseQts MapQts(Implementation.Dtos.QtsInfo source) =>
        new() { Awarded = source.HoldsFrom, CertificateUrl = source.CertificateUrl, StatusDescription = source.StatusDescription };

    private GetTeacherResponseEyts MapEyts(Implementation.Dtos.EytsInfo source) =>
        new() { Awarded = source.HoldsFrom, CertificateUrl = source.CertificateUrl, StatusDescription = source.StatusDescription };

    private GetTeacherResponseInduction MapDqtInduction(GetPersonResultDqtInduction source) =>
        new()
        {
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            Status = MapDqtInductionStatus(source.Status),
            StatusDescription = source.StatusDescription,
            CertificateUrl = source.CertificateUrl,
            Periods = source.Periods.Select(p => new GetTeacherResponseInductionPeriod
            {
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Terms = p.Terms,
                AppropriateBody = p.AppropriateBody is { } ab
                    ? new GetTeacherResponseInductionPeriodAppropriateBody { Name = ab.Name }
                    : null
            }).AsReadOnly()
        };

    private GetTeacherResponseInitialTeacherTraining MapInitialTeacherTraining(GetPersonResultInitialTeacherTraining source) =>
        new()
        {
            Qualification = source.Qualification is { } q
                ? new GetTeacherResponseInitialTeacherTrainingQualification { Name = q.Name }
                : null,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            ProgrammeType = null,
            ProgrammeTypeDescription = null,
            Result = null,
            AgeRange = source.AgeRange is { } ar
                ? new GetTeacherResponseInitialTeacherTrainingAgeRange { Description = ar.Description }
                : null,
            Provider = source.Provider is { } p
                ? new GetTeacherResponseInitialTeacherTrainingProvider { Name = p.Name, Ukprn = p.Ukprn }
                : null,
            Subjects = source.Subjects
                .Select(s => new GetTeacherResponseInitialTeacherTrainingSubject { Code = s.Code, Name = s.Name })
                .AsReadOnly()
        };

    private SanctionInfo MapSanctionInfo(Implementation.Dtos.SanctionInfo source) =>
        new() { Code = source.Code, StartDate = source.StartDate };

    private AlertInfo MapAlertInfo(Implementation.Dtos.Alert source) =>
        new()
        {
            AlertType = AlertType.Prohibition,
            DqtSanctionCode = source.AlertType.DqtSanctionCode!,
            StartDate = source.StartDate,
            EndDate = source.EndDate
        };

    private NameInfo MapNameInfo(Implementation.Dtos.NameInfo source) =>
        new() { FirstName = source.FirstName, MiddleName = source.MiddleName, LastName = source.LastName };

    private static Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus MapDqtInductionStatus(Implementation.Dtos.DqtInductionStatus source) =>
        source switch
        {
            Implementation.Dtos.DqtInductionStatus.Exempt => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Exempt,
            Implementation.Dtos.DqtInductionStatus.Fail => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Fail,
            Implementation.Dtos.DqtInductionStatus.FailedInWales => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.FailedinWales,
            Implementation.Dtos.DqtInductionStatus.InductionExtended => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.InductionExtended,
            Implementation.Dtos.DqtInductionStatus.InProgress => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.InProgress,
            Implementation.Dtos.DqtInductionStatus.NotYetCompleted => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.NotYetCompleted,
            Implementation.Dtos.DqtInductionStatus.Pass => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Pass,
            Implementation.Dtos.DqtInductionStatus.PassedInWales => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.PassedinWales,
            Implementation.Dtos.DqtInductionStatus.RequiredToComplete => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.RequiredtoComplete,
            _ => throw new ArgumentOutOfRangeException(nameof(source))
        };
}
